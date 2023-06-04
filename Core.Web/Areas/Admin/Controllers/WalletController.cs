using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.System;
using Core.Areas.Admin.Controllers;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Infrastructure.Telegram;
using Core.Services;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Core.Utilities.Helpers;
using Core.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Core.Web.Areas.Admin.Controllers
{
    public class WalletController : BaseController
    {
        private static readonly object balanceLock = new object();
        private static Dictionary<string, DateTime> _requestedUsers;

        static Dictionary<string, DateTime> RequestedUsers
        {
            get
            {
                if (_requestedUsers == null)
                {
                    lock (balanceLock)
                    {
                        _requestedUsers = new Dictionary<string, DateTime>();
                    }
                }
                return _requestedUsers;
            }
        }

        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<WalletController> _logger;
        private readonly IUserService _userService;
        private readonly IBlockChainService _blockChainService;
        private readonly IEmailSender _emailSender;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly ITicketTransactionService _ticketTransactionService;
        private readonly IConfigService _configService;
        private readonly AddressUtil _addressUtil = new();
        public WalletController(
            ILogger<WalletController> logger,
            UserManager<AppUser> userManager,
            IUserService userService,
            IEmailSender emailSender,
            IBlockChainService blockChainService,
            TelegramBotWrapper botTelegramService,
            IWalletTransactionService walletTransactionService,
            ITicketTransactionService ticketTransactionService,
            IConfigService configService
            )
        {
            _configService = configService;
            _logger = logger;
            _userManager = userManager;
            _userService = userService;
            _blockChainService = blockChainService;
            _emailSender = emailSender;
            _botTelegramService = botTelegramService;
            _walletTransactionService = walletTransactionService;
            _ticketTransactionService = ticketTransactionService;
        }

        public async Task<IActionResult> Index()
        {
            var appUser = await _userService.GetById(CurrentUserId.ToString());

            ViewBag.PublishKey = appUser.PublishKey;
            ViewBag.Enabled2FA = appUser.Enabled2FA;

            var token = $"{CurrentUserId}_{DateTime.Now}";

            var crypt = AesOperation.EncryptString(token);

            ViewBag.EncryptKey = HttpUtility.UrlEncode(crypt);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferFuture([FromBody] WithdrawViewModel model)
        {
            if (model.Amount <= 0)
                return new OkObjectResult(new GenericResult(false, "Invalid value"));

            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

            switch (model.Unit)
            {
                case Unit.Token:
                    if (model.Amount > appUser.BCAmount)
                        return new OkObjectResult(new GenericResult(false, "Insufficient account balance"));
                    appUser.BCAmount -= model.Amount;
                    appUser.BCFutureAmount += model.Amount;

                    break;
                case Unit.USDT:
                    if (model.Amount > appUser.USDTAmount)
                        return new OkObjectResult(new GenericResult(false, "Insufficient account balance"));
                    appUser.USDTAmount -= model.Amount;
                    appUser.USDTFutureAmount += model.Amount;
                    break;
                case Unit.PI:
                    if (model.Amount > appUser.PINetworkAmount)
                        return new OkObjectResult(new GenericResult(false, "Insufficient account balance"));
                    appUser.PINetworkAmount -= model.Amount;
                    appUser.PiNetworkFutureAmount += model.Amount;
                    break;

                default:
                    break;
            }

            var result = await _userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                var txnHash = Guid.NewGuid().ToString("N");

                _walletTransactionService.AddTransaction(
                                    appUser.Id, model.Amount, model.Amount, WalletTransactionType.TransferFuture,
                                    $"WALLET {model.Unit.GetDescription()}", $"WALLET FUTURE {model.Unit.GetDescription()}",
                                    model.Unit, 0, 0, txnHash);
                return new OkObjectResult(new GenericResult(true,
                        $"Transfer from Main account to future account " +
                        $"{model.Unit.GetDescription()} successful"));
            }

            return new OkObjectResult(new GenericResult(false, "Transfer failed"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferMain([FromBody] WithdrawViewModel model)
        {
            if (model.Amount <= 0)
                return new OkObjectResult(new GenericResult(false, "Invalid value"));

            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

            switch (model.Unit)
            {
                case Unit.Token:
                    if (model.Amount > appUser.BCFutureAmount)
                        return new OkObjectResult(new GenericResult(false, "Insufficient account balance"));
                    appUser.BCAmount += model.Amount;
                    appUser.BCFutureAmount -= model.Amount;

                    break;
                case Unit.USDT:
                    if (model.Amount > appUser.USDTFutureAmount)
                        return new OkObjectResult(new GenericResult(false, "Insufficient account balance"));
                    appUser.USDTAmount += model.Amount;
                    appUser.USDTFutureAmount -= model.Amount;
                    break;


                default:
                    break;
            }

            var result = await _userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                var txnHash = Guid.NewGuid().ToString("N");

                _walletTransactionService.AddTransaction(
                                    appUser.Id, model.Amount, model.Amount, WalletTransactionType.TransferMain,
                                    $"WALLET FUTURE {model.Unit.GetDescription()}", $"WALLET {model.Unit.GetDescription()}",
                                    model.Unit, 0, 0, txnHash);
                return new OkObjectResult(new GenericResult(true,
                        $"Transfer from Future account to Main account " +
                        $"{model.Unit.GetDescription()} successful"));
            }

            return new OkObjectResult(new GenericResult(false, "Transfer failed"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(
            [FromBody] WithdrawViewModel model, [FromQuery] string authenticatorCode)
        {
            try
            {
                if (model.Amount <= 0)
                    return new OkObjectResult(new GenericResult(false, "Invalid value"));

                var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());
                if (appUser == null)
                {
                    return new OkObjectResult(new GenericResult(false, "Account does not exist"));
                }

                var isMatched = await _userManager.CheckPasswordAsync(appUser, model.Password);
                if (!isMatched)
                {
                    return new OkObjectResult(new GenericResult(false, "Wrong password"));
                }

                if (appUser.TwoFactorEnabled)
                {
                    var isValid = await VerifyCode(authenticatorCode, _userManager, appUser);
                    if (!isValid)
                    {
                        return new OkObjectResult(new GenericResult(false, "Invalid authenticator code"));
                    }
                }

                if (appUser.IsShowOff)
                {
                    return new OkObjectResult(new GenericResult(false, "Account does not allow to withdraw"));
                }

                if (appUser.IsRejectWithdraw)
                    return new OkObjectResult(new GenericResult(false, "Account does not allow to withdraw"));


                if (string.IsNullOrWhiteSpace(model.ReceiveAddress))
                    return new OkObjectResult(new GenericResult(false, "Receive Address is required."));
                else
                {
                    model.ReceiveAddress = _addressUtil.ConvertToChecksumAddress(model.ReceiveAddress);

                    if (!_addressUtil.IsChecksumAddress(model.ReceiveAddress)
                                        || !_addressUtil.IsValidAddressLength(model.ReceiveAddress))
                    {
                        return new OkObjectResult(new GenericResult(false,
                                "Address not in standard format BSC BEP20."));
                    }
                }

                decimal feeWithdraw = 0;
                decimal minWithdraw = 0;
                decimal blance = 0;

                if (model.Unit == Unit.Token)
                {
                    feeWithdraw = CommonConstants.TokenFeeWithdraw;
                    minWithdraw = CommonConstants.TokenMinWithdraw;
                    blance = appUser.BCAmount;
                }
                else
                {
                    feeWithdraw = CommonConstants.USDTFeeWithdraw;
                    minWithdraw = CommonConstants.USDTMinWithdraw;
                    blance = appUser.USDTAmount;
                }

                if (model.Amount < minWithdraw)
                {
                    return new OkObjectResult(new GenericResult(false,
                        $"Minimum withdraw {minWithdraw} {model.Unit.GetDescription()}"));
                }


                if (model.Amount > blance)
                {
                    return new OkObjectResult(new GenericResult(false,
                        "Your balance is not enough to make a transaction"));
                }

                if (model.Unit == Unit.Token)
                {
                    appUser.BCAmount -= model.Amount;
                }
                else
                {
                    appUser.USDTAmount -= model.Amount;
                }

                var resulUpdate = await _userManager.UpdateAsync(appUser);

                if (!resulUpdate.Succeeded)
                {
                    _logger.LogError("Withdraw: {0}",
                        $"Payment failed {appUser.Id} {appUser.Email} " +
                        $"{model.Unit.GetDescription()} {model.Amount}");

                    return new OkObjectResult(new GenericResult(false, "Withdraw fail,retry later"));
                }

                decimal fee = feeWithdraw / 100;
                decimal feeAmount = model.Amount * fee;
                decimal amountReceive = model.Amount - feeAmount;

                var ticketTransaction = new TicketTransactionViewModel
                {
                    Amount = model.Amount,
                    Fee = feeWithdraw,
                    FeeAmount = feeAmount,
                    AmountReceive = amountReceive,
                    AddressFrom = CommonConstants.TransferPuKey,
                    AddressTo = model.ReceiveAddress,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow,
                    AppUserId = appUser.Id,
                    Unit = model.Unit,
                    Status = TicketTransactionStatus.Pending,
                    Type = TicketTransactionType.Withdraw
                };

                ticketTransaction.Id = _ticketTransactionService.Add(ticketTransaction);

                var sponsorEmail = string.Empty;

                if (appUser.ReferralId.HasValue)
                {
                    var sponsor = await _userManager.FindByIdAsync(appUser.ReferralId.Value.ToString());
                    sponsorEmail = sponsor.Email;
                }

                var withdrawMessage = TelegramBotHelper
                    .BuildErrorWithdrawMessage(new WithdrawMessageParam
                    {
                        Amount = ticketTransaction.AmountReceive,
                        CreatedDate = DateTime.UtcNow,
                        Email = appUser.Email,
                        WalletFrom = CommonConstants.TransferPuKey,
                        WalletTo = ticketTransaction.AddressTo,
                        Currency = ticketTransaction.Unit.GetDescription(),
                        FeeAmount = ticketTransaction.FeeAmount,
                        SponsorEmail = sponsorEmail,
                        Remarks = $"PENDING WITHDRAW".ToUpper()
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);

                //var result = await ProcesAutoPaymentAsync(ticketTransaction, appUser, sponsorEmail);

                //if (result)
                //{
                //    var withdrawMessage = TelegramBotHelper
                //    .BuildReportAutoWithdrawMessage(new WithdrawMessageParam
                //    {
                //        Amount = amountReceive,
                //        CreatedDate = DateTime.UtcNow,
                //        Email = appUser.Email,
                //        WalletTo = ticketTransaction.AddressTo,
                //        WalletFrom = CommonConstants.TransferPuKey,
                //        Currency = model.Unit.GetDescription(),
                //        FeeAmount = feeAmount,
                //        SponsorEmail = sponsorEmail
                //    });

                //    _logger.LogError("Withdraw: {0}",
                //        $"Payment success {appUser.Id} {appUser.Email} " +
                //        $"{model.Unit.GetDescription()} {model.Amount}");

                //    await _botTelegramService.SendMessageAsyncWithSendingBalance(
                //        TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);
                //}
                //else
                //{
                //    _logger.LogError("Withdraw: {0}",
                //        $"Payment failed {appUser.Id} {appUser.Email} " +
                //        $"{model.Unit.GetDescription()} {model.Amount}");

                //    var withdrawMessage = TelegramBotHelper
                //    .BuildReportAutoWithdrawExceptionMessage(new WithdrawMessageParam
                //    {
                //        Amount = model.Amount,
                //        CreatedDate = DateTime.UtcNow,
                //        Email = CurrentUserName,
                //        Currency = model.Unit.GetDescription(),
                //        Remarks = "AUTO WITHDRAW FAILED"
                //    });

                //    await _botTelegramService.SendMessageAsyncWithSendingBalance(
                //        TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);
                //}

                return new OkObjectResult(new GenericResult(true,
                        $"Create request withdraw from Wallet " +
                        $"{model.Unit.GetDescription()} successful"));
            }
            catch (Exception ex)
            {
                var withdrawMessage = TelegramBotHelper
                    .BuildReportAutoWithdrawExceptionMessage(new WithdrawMessageParam
                    {
                        Amount = model.Amount,
                        CreatedDate = DateTime.UtcNow,
                        Email = CurrentUserName,
                        Currency = model.Unit.GetDescription(),
                        Remarks = ex.Message
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);

                _logger.LogError("WalletController_Withdraw: {0}", ex.Message);

                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        async Task<bool> ProcesAutoPaymentAsync(
            TicketTransactionViewModel ticketTransaction, AppUser appUser, string sponsorEmail)
        {

            var isPendingTransaction = _ticketTransactionService.IsAnyPendingWithdraw(ticketTransaction.Id);
            if (isPendingTransaction)
            {
                var withdrawMessage = TelegramBotHelper
                    .BuildErrorWithdrawMessage(new WithdrawMessageParam
                    {
                        Amount = ticketTransaction.AmountReceive,
                        CreatedDate = DateTime.UtcNow,
                        Email = appUser.Email,
                        WalletFrom = CommonConstants.TransferPuKey,
                        WalletTo = ticketTransaction.AddressTo,
                        Currency = ticketTransaction.Unit.GetDescription(),
                        FeeAmount = ticketTransaction.FeeAmount,
                        SponsorEmail = sponsorEmail,
                        Remarks = "PENDING DUE TO already other pending transactions".ToUpper()
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);

                return false;
            }


            var isValidBalance = await _ticketTransactionService
                                        .IsPrimaryWalletEnoughBalanceAsync(ticketTransaction.Id);

            if (!isValidBalance)
            {
                var withdrawMessage = TelegramBotHelper
                    .BuildErrorWithdrawMessage(new WithdrawMessageParam
                    {
                        Amount = ticketTransaction.AmountReceive,
                        CreatedDate = DateTime.UtcNow,
                        Email = appUser.Email,
                        WalletFrom = CommonConstants.TransferPuKey,
                        WalletTo = ticketTransaction.AddressTo,
                        Currency = ticketTransaction.Unit.GetDescription(),
                        FeeAmount = ticketTransaction.FeeAmount,
                        SponsorEmail = sponsorEmail,
                        Remarks = "PENDING DUE TO Insufficient wallet balance to payment".ToUpper()
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);

                return false;
            }


            var isValidWithdrawAmount = _ticketTransactionService
                                        .IsValidDailyWithdrawAmount(ticketTransaction.Id);

            if (!isValidWithdrawAmount)
            {
                var withdrawMessage = TelegramBotHelper
                    .BuildErrorWithdrawMessage(new WithdrawMessageParam
                    {
                        Amount = ticketTransaction.AmountReceive,
                        CreatedDate = DateTime.UtcNow,
                        Email = appUser.Email,
                        WalletFrom = CommonConstants.TransferPuKey,
                        WalletTo = ticketTransaction.AddressTo,
                        Currency = ticketTransaction.Unit.GetDescription(),
                        FeeAmount = ticketTransaction.FeeAmount,
                        SponsorEmail = sponsorEmail,
                        Remarks = $"PENDING DUE TO MEET SYSTEM WITHDRAW LIMIT " +
                        $"= {(ticketTransaction.Unit == Unit.USDT ? CommonConstants.USDTMaxWithdraw : CommonConstants.TokenMaxWithdraw)} " +
                        $"{ticketTransaction.Unit.GetDescription()}".ToUpper()
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);

                return false;
            }

            var isValidWithdrawTimes = _ticketTransactionService
                                        .IsValidDailyWithdrawTimes(ticketTransaction.Id);

            if (!isValidWithdrawTimes)
            {
                var withdrawMessage = TelegramBotHelper
                    .BuildErrorWithdrawMessage(new WithdrawMessageParam
                    {
                        Amount = ticketTransaction.AmountReceive,
                        CreatedDate = DateTime.UtcNow,
                        Email = appUser.Email,
                        WalletFrom = CommonConstants.TransferPuKey,
                        WalletTo = ticketTransaction.AddressTo,
                        Currency = ticketTransaction.Unit.GetDescription(),
                        FeeAmount = ticketTransaction.FeeAmount,
                        SponsorEmail = sponsorEmail,
                        Remarks = $"PENDING DUE TO MEET SYSTEM WITHDRAW TIMES LIMIT " +
                        $"= {CommonConstants.TimeWithdraw} TIMES A DAY".ToUpper()
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);
            }

            var result = await _ticketTransactionService.Approved(ticketTransaction.Id);

            if (!result.Success)
            {
                _logger.LogError($"ProcesAutoPaymentAsync Error: {result.Message} - Transaction ID {ticketTransaction.Id}");

                var withdrawMessage = TelegramBotHelper
                    .BuildErrorWithdrawMessage(new WithdrawMessageParam
                    {
                        Amount = ticketTransaction.AmountReceive,
                        CreatedDate = DateTime.UtcNow,
                        Email = appUser.Email,
                        WalletFrom = CommonConstants.TransferPuKey,
                        WalletTo = ticketTransaction.AddressTo,
                        Currency = ticketTransaction.Unit.GetDescription(),
                        FeeAmount = ticketTransaction.FeeAmount,
                        SponsorEmail = sponsorEmail,
                        Remarks = $"PENDING DUE AUTO PAYMENT FAILED {result.Message} ".ToUpper()
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Withdraw, withdrawMessage, TelegramBotHelper.WithdrawGroup);
            }

            return result.Success;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFilter("GetWallets", NoOfRequest = 100, Seconds = 10)]
        public async Task<IActionResult> GetWallets(string token)
        {
            try
            {
                var rawToken = HttpUtility.UrlDecode(token);

                var decrypt = AesOperation.DecryptString(rawToken);

                var userId = decrypt.Split("_")[0];
                var date = DateTime.Parse(decrypt.Split("_")[1]);

                if (CurrentUserId.ToString() != userId)
                    return new OkObjectResult(new GenericResult(false, "failed"));

                if (date.AddMinutes(3) < DateTime.Now)
                    return new OkObjectResult(new GenericResult(false, "failed"));

                return await HandleRequestingUser(CurrentUserId.ToString(), async (appUser) =>
                {

                    _logger.LogInformation("Actual call RPC Wallet");

                    #region GET BNB
                    decimal bnbMainBalance = await _blockChainService.GetEtherBalance(
                        appUser.PublishKey, CommonConstants.Url);

                    if (bnbMainBalance >= CommonConstants.BNBMinDeposit)
                    {
                        decimal balanceTransfer = Math.Round(bnbMainBalance - 0.003m, 4);

                        _logger.LogInformation($"Start to Deposit {Unit.BNB.GetDescription()} - UserId {0}", userId);

                        var transactionHash = await _blockChainService.SendEthWithoutReceiptAsync(
                                                                    appUser.PrivateKey,
                                                                    CommonConstants.ReceivePuKey,
                                                                    balanceTransfer,
                                                                    CommonConstants.Url
                                                                    );

                        TransactionReceipt receipt = null;

                        int retriesCount = 0;

                        do
                        {
                            Thread.Sleep(3000);

                            receipt = await _blockChainService.GetTransactionReceiptByTransactionID(transactionHash,
                                CommonConstants.Url);

                            if (receipt != null && receipt.Status.Value == 1)
                                break;

                            retriesCount++;

                        } while (retriesCount < 3);

                        if (receipt != null && receipt.Status.Value == 1)
                        {
                            appUser.BNBAmount += balanceTransfer;

                            var updateUser = await _userManager.UpdateAsync(appUser);

                            if (updateUser.Succeeded)
                            {
                                _walletTransactionService.AddTransaction(
                                    appUser.Id, balanceTransfer, balanceTransfer, WalletTransactionType.Deposit,
                                    appUser.PublishKey, CommonConstants.ReceivePuKey,
                                    Unit.BNB, 0, 0, transactionHash);
                            }

                            await SendBotDeposit(appUser, balanceTransfer, Unit.BNB);
                        }
                        else
                        {
                            _logger.LogError($"GetWallets Deposit {Unit.BNB.GetDescription()} failed AppUser:{appUser.Id}" +
                                $" Deposit Balance:{balanceTransfer}" +
                                $" TxnHash: {transactionHash}");
                        }
                    }
                    #endregion

                    #region GET Project Token

                    decimal tokenBalance = await _blockChainService.GetERC20Balance(
                        appUser.PublishKey, CommonConstants.TokenContract,
                        CommonConstants.TokenDecimals, CommonConstants.Url);

                    if (tokenBalance >= CommonConstants.TokenMinDeposit)
                    {
                        var bnbBalance = await _blockChainService
                                                .GetEtherBalance(appUser.PublishKey, CommonConstants.Url);

                        if (bnbBalance < 0.003m)
                        {
                            _logger.LogInformation("Start to transfer Fee BNB 0.003m - UserId {0}", userId);

                            await _blockChainService.SendEthWithoutReceiptAsync(
                                CommonConstants.TransferPrKey, appUser.PublishKey,
                                0.003m, CommonConstants.Url);

                            Thread.Sleep(5000); // waiting to completed transfer
                        }

                        _logger.LogInformation($"Start to Deposit {Unit.Token.GetDescription()} - UserId {0}", userId);


                        var transactionHash = await _blockChainService.SendERC20WithoutReceiptAsync(
                                                                appUser.PrivateKey,
                                                                CommonConstants.ReceivePuKey,
                                                                CommonConstants.TokenContract,
                                                                tokenBalance,
                                                                CommonConstants.TokenDecimals,
                                                                CommonConstants.Url
                                                                );

                        TransactionReceipt receipt = null;

                        int retriesCount = 0;

                        do
                        {
                            Thread.Sleep(3000);

                            receipt = await _blockChainService.GetTransactionReceiptByTransactionID(transactionHash,
                                CommonConstants.Url);

                            if (receipt != null && receipt.Status.Value == 1)
                                break;

                            retriesCount++;

                        } while (retriesCount < 3);


                        if (receipt != null && receipt.Status.Value == 1)
                        {
                            appUser.BCAmount += tokenBalance;

                            var updateUser = await _userManager.UpdateAsync(appUser);

                            if (updateUser.Succeeded)
                            {
                                _walletTransactionService.AddTransaction(
                                    appUser.Id, tokenBalance, tokenBalance, WalletTransactionType.Deposit,
                                    appUser.PublishKey, CommonConstants.ReceivePuKey,
                                    Unit.Token, 0, 0, transactionHash);

                            }

                            await SendBotDeposit(appUser, tokenBalance, Unit.Token);
                        }
                        else
                        {
                            _logger.LogError($"GetWallets Deposit {CommonConstants.TOKEN_CODE} failed AppUser:{appUser.Id}" +
                                $" Deposit Balance:{bnbBalance}" +
                                $" TxnHash: {transactionHash}");
                        }
                    }
                    #endregion

                    #region GET USDT
                    decimal usdtBalance = await _blockChainService.GetERC20Balance(
                        appUser.PublishKey, CommonConstants.USDTContract,
                        CommonConstants.USDTDecimals, CommonConstants.Url);

                    if (usdtBalance >= CommonConstants.USDTMinDeposit)
                    {
                        var bnbBalance = await _blockChainService
                                                .GetEtherBalance(appUser.PublishKey, CommonConstants.Url);

                        if (bnbBalance < 0.003m)
                        {
                            _logger.LogInformation("Start to transfer Fee BNB 0.003m - UserId {0}", userId);

                            var transaction = await _blockChainService.SendEthWithoutReceiptAsync(
                                CommonConstants.TransferPrKey, appUser.PublishKey,
                                0.003m, CommonConstants.Url);

                            Thread.Sleep(5000);
                        }

                        _logger.LogInformation($"Start to Deposit {Unit.USDT.GetDescription()} - UserId {0}", userId);

                        var transactionHash = await _blockChainService.SendERC20WithoutReceiptAsync(
                                                                    appUser.PrivateKey,
                                                                    CommonConstants.ReceivePuKey,
                                                                    CommonConstants.USDTContract,
                                                                    usdtBalance,
                                                                    CommonConstants.USDTDecimals,
                                                                    CommonConstants.Url
                                                                    );

                        TransactionReceipt receipt = null;

                        int retriesCount = 0;

                        do
                        {
                            Thread.Sleep(3000);

                            receipt = await _blockChainService.GetTransactionReceiptByTransactionID(transactionHash,
                                CommonConstants.Url);

                            if (receipt != null && receipt.Status.Value == 1)
                                break;

                            retriesCount++;

                        } while (retriesCount < 3);

                        if (receipt != null && receipt.Status.Value == 1)
                        {
                            appUser.USDTAmount += usdtBalance;

                            var updateUser = await _userManager.UpdateAsync(appUser);

                            if (updateUser.Succeeded)
                            {
                                _walletTransactionService.AddTransaction(
                                    appUser.Id, usdtBalance, usdtBalance, WalletTransactionType.Deposit,
                                    appUser.PublishKey, CommonConstants.ReceivePuKey,
                                    Unit.USDT, 0, 0, transactionHash);
                            }

                            await SendBotDeposit(appUser, usdtBalance, Unit.USDT);
                        }
                        else
                        {
                            _logger.LogError($"GetWallets Deposit {Unit.USDT.GetDescription()} failed AppUser:{appUser.Id}" +
                                $" Deposit Balance:{bnbBalance}" +
                                $" TxnHash: {transactionHash}");
                        }
                    }
                    #endregion

                    #region GET BUSD
                    decimal busdBalance = await _blockChainService.GetERC20Balance(
                        appUser.PublishKey, CommonConstants.BUSDContract,
                        CommonConstants.BUSDDecimals, CommonConstants.Url);

                    if (busdBalance >= CommonConstants.BUSDMinDeposit)
                    {
                        var bnbBalance = await _blockChainService
                                                .GetEtherBalance(appUser.PublishKey, CommonConstants.Url);

                        if (bnbBalance < 0.003m)
                        {
                            _logger.LogInformation("Start to transfer Fee BNB 0.003m - UserId {0}", userId);

                            var transaction = await _blockChainService.SendEthWithoutReceiptAsync(
                                CommonConstants.TransferPrKey, appUser.PublishKey,
                                0.003m, CommonConstants.Url);

                            Thread.Sleep(5000);
                        }

                        _logger.LogInformation($"Start to Deposit {Unit.BUSD.GetDescription()} - UserId {0}", userId);

                        var transactionHash = await _blockChainService.SendERC20WithoutReceiptAsync(
                                                                    appUser.PrivateKey,
                                                                    CommonConstants.ReceivePuKey,
                                                                    CommonConstants.BUSDContract,
                                                                    busdBalance,
                                                                    CommonConstants.BUSDDecimals,
                                                                    CommonConstants.Url
                                                                    );

                        TransactionReceipt receipt = null;

                        int retriesCount = 0;

                        do
                        {
                            Thread.Sleep(3000);

                            receipt = await _blockChainService.GetTransactionReceiptByTransactionID(transactionHash,
                                CommonConstants.Url);

                            if (receipt != null && receipt.Status.Value == 1)
                                break;

                            retriesCount++;

                        } while (retriesCount < 3);

                        if (receipt != null && receipt.Status.Value == 1)
                        {
                            appUser.BUSDAmount += busdBalance;

                            var updateUser = await _userManager.UpdateAsync(appUser);

                            if (updateUser.Succeeded)
                            {
                                _walletTransactionService.AddTransaction(
                                    appUser.Id, busdBalance, busdBalance, WalletTransactionType.Deposit,
                                    appUser.PublishKey, CommonConstants.ReceivePuKey,
                                    Unit.BUSD, 0, 0, transactionHash);
                            }

                            await SendBotDeposit(appUser, busdBalance, Unit.BUSD);
                        }
                        else
                        {
                            _logger.LogError($"GetWallets Deposit {Unit.BUSD.GetDescription()} failed AppUser:{appUser.Id}" +
                                $" Deposit Balance:{busdBalance}" +
                                $" TxnHash: {transactionHash}");
                        }
                    }
                    #endregion

                    var model = new WalletViewModel()
                    {
                        PublishKey = appUser.PublishKey,
                        USDTAmount = appUser.USDTAmount,
                        BCAmount = appUser.BCAmount,
                        BUSDAmount = appUser.BUSDAmount,
                        SHIBAmount = appUser.ShibaAmount,
                        BNBAmount = appUser.BNBAmount,
                        BotTradeAmount = appUser.BotTradeAmount,
                        BCClaimAmount = appUser.BCClaimAmount,
                        SHIBClaimAmount = appUser.SHIBClaimAmount,
                        BCFutureAmount = appUser.BCFutureAmount,
                        USDTFutureAmount = appUser.USDTFutureAmount,
                        PiNetworkAmount = appUser.PINetworkAmount,
                        PiNetworkFutureAmount = appUser.PiNetworkFutureAmount,
                    };

                    _logger.LogInformation("End GetBalance, UserId {0}", userId);

                    return new OkObjectResult(new GenericResult(true, model));
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetWallets {@0}", ex);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        public async Task SendBotDeposit(AppUser appUser, decimal balance, Unit unit)
        {
            try
            {
                var sponsorEmail = string.Empty;

                if (appUser.ReferralId.HasValue)
                {
                    var sponsor = await _userManager.FindByIdAsync(appUser.ReferralId.Value.ToString());
                    sponsorEmail = sponsor.Email;
                }

                var depositMessage = TelegramBotHelper
                    .BuildReportDepositMessage(new DepositMessageParam
                    {
                        Amount = balance,
                        CreatedDate = DateTime.UtcNow,
                        WalletFrom = appUser.PublishKey,
                        WalletTo = CommonConstants.ReceivePuKey,
                        Email = appUser.Email,
                        Currency = unit.GetDescription(),
                        SponsorEmail = sponsorEmail
                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(
                    TelegramBotActionType.Deposit, depositMessage, TelegramBotHelper.DepositGroup);
            }
            catch (Exception ex)
            {

            }
        }


        private async Task<OkObjectResult> HandleRequestingUser(string userId, Func<AppUser, Task<OkObjectResult>> handleGetBalance)
        {
            _logger.LogInformation("Start Handling Requesting User, UserId {0}", userId);

            try
            {
                bool hasUser = false;

                lock (balanceLock)
                {
                    //check if user not existing , add to dicts , has user = false
                    if (RequestedUsers.ContainsKey(userId))
                    {
                        hasUser = true;

                        var addAt = RequestedUsers[userId];

                        if ((DateTime.UtcNow - addAt).TotalMinutes > 5) // user stuck in cache , to remove then keep hasUser = false
                        {
                            RequestedUsers.Remove(userId);
                            RequestedUsers.Add(userId, DateTime.UtcNow);
                            hasUser = false;
                        }
                    }
                    else
                    {
                        RequestedUsers.Add(userId, DateTime.UtcNow);
                    }
                }

                var appUser = await _userManager.FindByIdAsync(userId);

                if (!hasUser)
                {
                    var result = await handleGetBalance(appUser);

                    lock (balanceLock)
                    {
                        if (RequestedUsers.ContainsKey(userId))
                            RequestedUsers.Remove(userId);
                    }

                    return result;
                }

                var model = new WalletViewModel()
                {
                    PublishKey = appUser.PublishKey,
                    USDTAmount = appUser.USDTAmount,
                    BCAmount = appUser.BCAmount
                };

                _logger.LogInformation("End Handling Requesting User, UserId {0}", userId);

                return new OkObjectResult(new GenericResult(true, model));
            }
            catch (Exception e)
            {
                _logger.LogInformation("Start Handling Requesting User, UserId {0} Exception ", e.StackTrace);

                if (RequestedUsers.ContainsKey(userId))
                    RequestedUsers.Remove(userId);
            }

            return new OkObjectResult(new GenericResult(true));
        }

        [HttpGet]
        public IActionResult GetAllTicketPaging(string keyword, int page, int pageSize)
        {
            var model = _ticketTransactionService
                .GetAllPaging(keyword, CurrentUserName, 0, false, page, pageSize);

            return new OkObjectResult(model);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequestFilter("ClaimDailyReward", NoOfRequest = 1, Seconds = 5)]
        public async Task<IActionResult> ClaimDailyReward()
        {
            var result = await _walletTransactionService.ProcessClaimDailyReward(CurrentUserId);

            return new OkObjectResult(result);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetClaimDailyStatus()
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

            bool isClaimed = appUser.LastClaimReward != null && appUser.LastClaimReward.Value.AddHours(24) >= DateTime.Now;

            return new OkObjectResult(new
            {
                IsClaimed = isClaimed,
                NextClaim = isClaimed ? appUser.LastClaimReward.Value.AddHours(24).ToString("yyyy/MM/dd HH:mm:ss") : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenWallet(Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            var balance = 0m;
            switch (unit)
            {
                case Unit.Token:
                    balance = appUser.BCAmount;
                    break;
                case Unit.USDT:
                    balance = appUser.USDTAmount;
                    break;
                default:
                    break;
            }

            return new OkObjectResult(new
            {
                Balance = balance
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenWalletFuture(Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            var balance = 0m;
            switch (unit)
            {
                case Unit.Token:
                    balance = appUser.BCFutureAmount;
                    break;
                case Unit.USDT:
                    balance = appUser.USDTFutureAmount;
                    break;
                default:
                    break;
            }

            return new OkObjectResult(new
            {
                Balance = balance
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenWalletSwap(Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            var balance = 0m;
            switch (unit)
            {
                case Unit.Token:
                    balance = appUser.BCFutureAmount;
                    break;
                case Unit.USDT:
                    balance = appUser.USDTFutureAmount;
                    break;
                case Unit.PI:
                    balance = appUser.PINetworkAmount;
                    break;
                default:
                    break;
            }

            return new OkObjectResult(new
            {
                Balance = balance
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Swap([FromBody] WithdrawViewModel model)
        {
            if (model.Amount <= 0)
                return new OkObjectResult(new GenericResult(false, "Invalid value"));

            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());
            var usdtAmount = 0m;
            switch (model.Unit)
            {
                case Unit.Token:
                    
                    break;

                case Unit.PI:

                    if (model.Amount > appUser.PINetworkAmount)
                        return new OkObjectResult(new GenericResult(false, "Insufficient account balance"));

                    var tokenPrice = await _blockChainService.GetPiNetworkPrice();
                    usdtAmount = model.Amount / tokenPrice;

                    appUser.USDTAmount += usdtAmount;
                    appUser.PINetworkAmount -= model.Amount;

                    break;
                
                default:
                    break;
            }

            var result = await _userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                var txnHash = Guid.NewGuid().ToString("N");

                _walletTransactionService.AddTransaction(
                                    appUser.Id, model.Amount, 0, WalletTransactionType.SwapToUSDT,
                                    $"WALLET {model.Unit.GetDescription()}", $"WALLET {Unit.USDT.GetDescription()}",
                                    model.Unit, 0, 0, txnHash);

                _walletTransactionService.AddTransaction(
                                    appUser.Id, usdtAmount, usdtAmount, WalletTransactionType.SwapToUSDT,
                                    $"WALLET {model.Unit.GetDescription()}", $"WALLET {Unit.USDT.GetDescription()}",
                                    Unit.USDT, 0, 0, txnHash);
                return new OkObjectResult(new GenericResult(true,
                        $"Swap from {model.Unit.GetDescription()} account to {Unit.USDT.GetDescription()} account " +
                        $"{model.Unit.GetDescription()} successful"));
            }

            return new OkObjectResult(new GenericResult(false, "Swap failed"));
        }
    }
}

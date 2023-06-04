using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.System;
using Core.Application.ViewModels.Transfer;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Core.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Areas.Admin.Controllers
{
    public class TransferController : BaseController
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<TransferController> _logger;
        private readonly IUserService _userService;
        private readonly IBlockChainService _blockChainService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ITicketTransactionService _ticketTransactionService;
        public TransferController(
            ILogger<TransferController> logger,
            UserManager<AppUser> userManager,
            IUserService userService,
            IBlockChainService blockChainService,
            IWalletTransactionService walletTransactionService,
            ITicketTransactionService ticketTransactionService
            )
        {
            _logger = logger;
            _userManager = userManager;
            _userService = userService;
            _blockChainService = blockChainService;
            _walletTransactionService = walletTransactionService;
            _ticketTransactionService = ticketTransactionService;
        }

        public async Task<IActionResult> Internal()
        {
            var appUser = await _userService.GetById(CurrentUserId.ToString());

            ViewBag.Enabled2FA = appUser.Enabled2FA;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(
            [FromBody] TransferViewModel model, [FromQuery] string authenticatorCode)
        {
            try
            {

                if (model.Amount <= 0)
                    return new OkObjectResult(new GenericResult(false, "Invalid value"));

                var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());
                if (appUser == null)
                    return new OkObjectResult(new GenericResult(false, "Account does not exist"));

                if (appUser.IsShowOff)
                    return new OkObjectResult(new GenericResult(false, "Account does not allow to transfer"));

                if (appUser.IsRejectTransfer)
                    return new OkObjectResult(new GenericResult(false, "Account does not allow to transfer"));


                var isMatched = await _userManager.CheckPasswordAsync(appUser, model.Password);
                if (!isMatched)
                    return new OkObjectResult(new GenericResult(false, "Wrong password"));

                if (appUser.TwoFactorEnabled)
                {
                    var isValid = await VerifyCode(authenticatorCode, _userManager, appUser);
                    if (!isValid)
                        return new OkObjectResult(new GenericResult(false, "Invalid authenticator code"));
                }

                var getTokenInfo = await GetBalance(model.Unit);

                if (!getTokenInfo.Success)
                    return new OkObjectResult(getTokenInfo);

                var tokenInfo = getTokenInfo.Data as TransferBalanceViewModel;

                if (model.Amount < tokenInfo.MinTransfer)
                    return new OkObjectResult(new GenericResult(false,
                        $"Minimum transfer {tokenInfo.MinTransfer} {model.Unit.GetDescription()}"));

                if (model.Amount > tokenInfo.Balance)
                    return new OkObjectResult(new GenericResult(false,
                        "Your balance is not enough to make a transaction"));

                if (string.IsNullOrEmpty(model.Sponsor) || model.Sponsor.Length <= 4)
                    return new OkObjectResult(new GenericResult(false, "Sponsor does not exists."));

                var sponsorString = model.Sponsor.GetRawSponsor();

                if (string.IsNullOrEmpty(sponsorString))
                    return new OkObjectResult(new GenericResult(false, "Sponsor does not exists."));

                var userSponsor = _userManager.Users.FirstOrDefault(x => x.Sponsor == sponsorString);
                if (userSponsor == null)
                    return new OkObjectResult(new GenericResult(false, "Sponsor does not exists."));

                //var isStaked = _walletTransactionService.IsStaked(CurrentUserId);
                //if (!isStaked)
                //    return new OkObjectResult(new GenericResult(false, "Account should invest bot trade to transfer"));

                var transferFeeAmount = model.Amount * (tokenInfo.TransferFee / 100);

                var receiveAmount = model.Amount - transferFeeAmount;

                switch (model.Unit)
                {
                    case Unit.Token:
                        appUser.BCAmount -= model.Amount;
                        userSponsor.BCAmount += receiveAmount;
                        break;
                    case Unit.USDT:
                        appUser.USDTAmount -= model.Amount;
                        userSponsor.USDTAmount += receiveAmount;
                        break;
                    case Unit.BUSD:
                        appUser.BUSDAmount -= model.Amount;
                        userSponsor.BUSDAmount += receiveAmount;
                        break;
                    case Unit.SHIBAINU:
                        appUser.ShibaAmount -= model.Amount;
                        userSponsor.ShibaAmount += receiveAmount;
                        break;
                    case Unit.BNB:
                        appUser.BNBAmount -= model.Amount;
                        userSponsor.BNBAmount += receiveAmount;
                        break;
                    default:
                        break;
                }

                var updateFromUser = await _userManager.UpdateAsync(appUser);

                if (updateFromUser.Succeeded)
                {
                    var txnHash = Guid.NewGuid().ToString("N");

                    var txn1 = new WalletTransactionViewModel()
                    {
                        AddressFrom = $"Wallet {model.Unit.GetDescription()}",
                        AddressTo = userSponsor.Email,
                        Amount = model.Amount,
                        Fee = tokenInfo.TransferFee,
                        FeeAmount = transferFeeAmount,
                        AmountReceive = receiveAmount,
                        AppUserId = appUser.Id,
                        TransactionHash = txnHash,
                        DateCreated = DateTime.UtcNow,
                        Unit = model.Unit,
                        Type = WalletTransactionType.Transfer,
                        Remarks = $"Transfer from {appUser.Email} to {userSponsor.Email}",
                    };

                    _walletTransactionService.Add(txn1);
                    _walletTransactionService.Save();

                    var updateToUser = await _userManager.UpdateAsync(userSponsor);

                    if (updateToUser.Succeeded)
                    {
                        var txn2 = new WalletTransactionViewModel()
                        {
                            AddressFrom = appUser.Email,
                            AddressTo = $"Wallet {model.Unit.GetDescription()}",
                            Amount = model.Amount,
                            Fee = tokenInfo.TransferFee,
                            FeeAmount = transferFeeAmount,
                            AmountReceive = receiveAmount,
                            AppUserId = userSponsor.Id,
                            TransactionHash = txnHash,
                            DateCreated = DateTime.UtcNow,
                            Unit = model.Unit,
                            Type = WalletTransactionType.Received,
                            Remarks = $"Transfer from {appUser.Email} to {userSponsor.Email}",
                        };

                        _walletTransactionService.Add(txn2);
                        _walletTransactionService.Save();
                    }
                    else
                    {
                        return new OkObjectResult(new GenericResult(false,
                       string.Join(",", updateToUser.Errors.Select(x => x.Description))));
                    }


                    return new OkObjectResult(new GenericResult(true,
                        $"Transfer from Wallet {model.Unit.GetDescription()} to {userSponsor.Email} is successful"));
                }
                else
                {
                    return new OkObjectResult(new GenericResult(false,
                       string.Join(",", updateFromUser.Errors.Select(x => x.Description))));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("TransferWallet: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferFuture(
            [FromBody] TransferViewModel model, [FromQuery] string authenticatorCode)
        {
            try
            {

                if (model.Amount <= 0)
                    return new OkObjectResult(new GenericResult(false, "Invalid amount"));

                var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

                if (appUser.IsShowOff)
                    return new OkObjectResult(new GenericResult(false, "Account does not allow to transfer"));

                if (appUser.IsRejectTransfer)
                    return new OkObjectResult(new GenericResult(false, "Account does not allow to transfer"));


                var isMatched = await _userManager.CheckPasswordAsync(appUser, model.Password);
                if (!isMatched)
                    return new OkObjectResult(new GenericResult(false, "Wrong password"));

                if (appUser.TwoFactorEnabled)
                {
                    var isValid = await VerifyCode(authenticatorCode, _userManager, appUser);
                    if (!isValid)
                        return new OkObjectResult(new GenericResult(false, "Invalid authenticator code"));
                }

                var getTokenInfo = await GetBalance(model.Unit);

                if (!getTokenInfo.Success)
                    return new OkObjectResult(getTokenInfo);

                var tokenInfo = getTokenInfo.Data as TransferBalanceViewModel;

                if (model.Amount < tokenInfo.MinTransfer)
                    return new OkObjectResult(new GenericResult(false,
                        $"Minimum transfer {tokenInfo.MinTransfer} {model.Unit.GetDescription()}"));

                if (model.Amount > tokenInfo.Balance)
                    return new OkObjectResult(new GenericResult(false,
                        "Your balance is not enough to make a transaction"));


                var transferFeeAmount = model.Amount * (tokenInfo.TransferFee / 100);

                var receiveAmount = model.Amount - transferFeeAmount;

                Unit unitFuture = 0;

                switch (model.Unit)
                {
                    case Unit.Token:
                        appUser.BCAmount -= model.Amount;
                        appUser.BCFutureAmount += model.Amount;
                        unitFuture = Unit.Token;
                        break;
                    case Unit.USDT:
                        appUser.USDTAmount -= model.Amount;
                        appUser.USDTFutureAmount += model.Amount;
                        unitFuture = Unit.USDT;
                        break;
                    //case Unit.BUSD:
                    //    appUser.BUSDAmount -= model.Amount;
                    //    userSponsor.BUSDAmount += receiveAmount;
                    //    break;
                    //case Unit.SHIBAINU:
                    //    appUser.ShibaAmount -= model.Amount;
                    //    userSponsor.ShibaAmount += receiveAmount;
                    //    break;
                    //case Unit.BNB:
                    //    appUser.BNBAmount -= model.Amount;
                    //    userSponsor.BNBAmount += receiveAmount;
                    //    break;
                    //default:
                    //    break;
                }

                var updateFromUser = await _userManager.UpdateAsync(appUser);

                if (updateFromUser.Succeeded)
                {
                    var txnHash = Guid.NewGuid().ToString("N");

                    var txn1 = new WalletTransactionViewModel()
                    {
                        AddressFrom = $"Wallet {model.Unit.GetDescription()}",
                        AddressTo = $"Wallet Claim {unitFuture.GetDescription()}",
                        Amount = model.Amount,
                        Fee = tokenInfo.TransferFee,
                        FeeAmount = transferFeeAmount,
                        AmountReceive = receiveAmount,
                        AppUserId = appUser.Id,
                        TransactionHash = txnHash,
                        DateCreated = DateTime.UtcNow,
                        Unit = model.Unit,
                        Type = WalletTransactionType.TransferFuture,
                        Remarks = $"Transfer from wallet  {model.Unit.GetDescription()} to wallet {unitFuture.GetDescription()}",
                    };

                    _walletTransactionService.Add(txn1);

                    var txn2 = new WalletTransactionViewModel()
                    {
                        AddressFrom = $"Wallet {model.Unit.GetDescription()}",
                        AddressTo = $"Wallet {unitFuture.GetDescription()}",
                        Amount = model.Amount,
                        Fee = tokenInfo.TransferFee,
                        FeeAmount = transferFeeAmount,
                        AmountReceive = receiveAmount,
                        AppUserId = appUser.Id,
                        TransactionHash = txnHash,
                        DateCreated = DateTime.UtcNow,
                        Unit = model.Unit,
                        Type = WalletTransactionType.Received,
                        Remarks = $"Transfer from wallet  {model.Unit.GetDescription()} to wallet {unitFuture.GetDescription()}",
                    };

                    _walletTransactionService.Add(txn2);

                    _walletTransactionService.Save();

                    return new OkObjectResult(new GenericResult(true,
                        $"Transfer from Wallet {model.Unit.GetDescription()} to {unitFuture.GetDescription()} is successful"));
                }
                else
                {
                    return new OkObjectResult(new GenericResult(false,
                       string.Join(",", updateFromUser.Errors.Select(x => x.Description))));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("TransferWallet: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        public async Task<GenericResult> GetBalance(Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

            decimal balance = 0;
            decimal feeAmount = 0;
            decimal minTransfer = 0;
            decimal futureBalance = 0;

            string errorMsg = "";

            switch (unit)
            {
                case Unit.Token:
                    balance = appUser.BCAmount;
                    futureBalance = appUser.BCFutureAmount;
                    feeAmount = CommonConstants.TokenFeeTransfer;
                    minTransfer = CommonConstants.TokenMinTransfer;
                    break;
                case Unit.USDT:
                    balance = appUser.USDTAmount;
                    futureBalance = appUser.USDTFutureAmount;
                    feeAmount = CommonConstants.USDTFeeTransfer;
                    minTransfer = CommonConstants.USDTMinTransfer;

                    break;
                case Unit.BUSD:
                    balance = appUser.BUSDAmount;
                    feeAmount = CommonConstants.BUSDFeeTransfer;
                    minTransfer = CommonConstants.BUSDMinTransfer;
                    break;
                case Unit.SHIBAINU:
                    balance = appUser.ShibaAmount;
                    feeAmount = CommonConstants.SHIBAFeeTransfer;
                    minTransfer = CommonConstants.SHIBAMinTransfer;
                    break;
                case Unit.BNB:
                    balance = appUser.BNBAmount;
                    feeAmount = CommonConstants.BNBFeeTransfer;
                    minTransfer = CommonConstants.BNBMinTransfer;
                    break;
                default:
                    break;
            }

            var model = new TransferBalanceViewModel()
            {
                Balance = balance,
                ErrorMsg = errorMsg,
                MinTransfer = minTransfer,
                TransferFee = feeAmount,
                FutureBalance = futureBalance,
            };

            return new GenericResult(true,model);
        }

        [HttpGet]
        public IActionResult GetSponsor(string sponsor)
        {
            try
            {
                if (string.IsNullOrEmpty(sponsor) || sponsor.Length <= 4)
                    return new OkObjectResult(new GenericResult(false, "Sponsor does not exists."));

                var sponsorString = sponsor.GetRawSponsor();

                if (string.IsNullOrEmpty(sponsorString))
                    return new OkObjectResult(new GenericResult(false, "Sponsor does not exists."));

                var userSponsor = _userManager.Users.FirstOrDefault(x => x.Sponsor.Equals(sponsorString));
                if (userSponsor == null)
                    return new OkObjectResult(new GenericResult(false, "Sponsor does not exists."));

                return new OkObjectResult(new GenericResult(true, userSponsor.Email));
            }
            catch (Exception ex)
            {
                _logger.LogError("GetWalletBlance {@0}", ex);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        public async Task<IActionResult> Future()
        {
            var appUser = await _userService.GetById(CurrentUserId.ToString());

            ViewBag.Enabled2FA = appUser.Enabled2FA;

            return View();
        }
    }
}

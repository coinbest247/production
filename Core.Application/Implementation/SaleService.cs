using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.Telegram;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Newtonsoft.Json;
using Polly.Retry;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Utilities.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Core.Data.EF.Repositories;

namespace Core.Application.Implementation
{
    public class SaleService : BaseService, ISaleService
    {
        private readonly IBlockChainService _blockChainService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SaleService> _logger;
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ISaleDefiRepository _saleDefiRepository;
        public const string DAppTransactionId = "DAppTransactionId";
        private readonly IConfigService _configService;
        private IConfiguration _configuration;
        private readonly ISaleAffiliateRepository _saleAffiliateRepository;

        private static AsyncRetryPolicy _policy = Policy
          .Handle<Exception>()
          .WaitAndRetryAsync(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(15)
              });

        public SaleService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<SaleService> logger,
            UserManager<AppUser> userManager,
            IBlockChainService blockChainService,
            TelegramBotWrapper botTelegramService,
            ISaleDefiRepository saleDefiRepository,
            IWalletTransactionService walletTransactionService,
            IConfigService configService,
            ISaleAffiliateRepository saleAffiliateRepository
            )
            : base(userManager)
        {
            _configService = configService;
            _logger = logger;
            _blockChainService = blockChainService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _botTelegramService = botTelegramService;
            _saleDefiRepository = saleDefiRepository;
            _walletTransactionService = walletTransactionService;
            _saleAffiliateRepository = saleAffiliateRepository;
        }

        #region Sale Defi
        public PagedResult<SaleDefiViewModel> GetAllPaging(string keyword, string address, int pageIndex, int pageSize)
        {
            if (string.IsNullOrEmpty(address))
            {
                address = string.Empty;
            }

            var query = _saleDefiRepository.FindAll(x => x.TransactionState == SaleDefiTransactionState.Confirmed);

            query = query.Where(x => x.AddressFrom.ToLower().Equals(address.ToLower()));

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.BNBTransactionHash.Contains(keyword)
                || x.TokenTransactionHash.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword));

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.BNBAmount)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new SaleDefiViewModel()
                {
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    DateCreated = x.DateCreated,
                    BNBAmount = x.BNBAmount,
                    TokenAmount = x.TokenAmount,
                    BNBTransactionHash = x.BNBTransactionHash,
                    TokenTransactionHash = x.TokenTransactionHash,
                    TransactionStateName = x.TransactionState.GetDescription(),
                    TransactionState = x.TransactionState,
                    USDAmount = x.USDAmount,
                    DateUpdated = x.DateUpdated,
                    Remarks = x.Remarks
                }).ToList();

            return new PagedResult<SaleDefiViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public async Task<(GenericResult result, string transactionId)>
            InitializeTransactionProgress(SaleInitializationParams model)
        {
            decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");

            if (priceBNBBep20 == 0)
            {
                return (new GenericResult(false,
                    "There is a problem loading the currency value!"), string.Empty);
            }

            var transaction = new SaleDefi
            {
                AddressFrom = model.Address,
                AddressTo = CommonConstants.ReceivePuKey,
                DateCreated = DateTime.Now,
                TransactionState = SaleDefiTransactionState.Requested,
                IsDevice = model.IsDevice,
                USDAmount = Math.Round(model.BNBAmount * priceBNBBep20, 2),
                WalletType = model.WalletType,
                BNBAmount = model.BNBAmount,
                TokenAmount = CalculateTokenAmount(model.BNBAmount, priceBNBBep20)
            };

            await _saleDefiRepository.AddAsync(transaction);

            _unitOfWork.Commit();

            // luu wallet transaction gom txnhash ,

            var message = new BlockchainParams
            {
                TransactionHex = transaction.Id.ToString().ToHexUTF8(),
                From = model.Address,
                To = CommonConstants.ReceivePuKey,
                Value = transaction.BNBAmount
            };

            return (GenericResult.ToSuccess("Successed to initialize Transaction", message), transaction.Id.ToString());
        }

        public async Task<GenericResult> ProcessVerificationBNBTransaction(
            string transactionHash)
        {
            try
            {

                _logger.LogInformation(
                    $"Start calling VerifyMetaMaskRequest with transaction hash: {transactionHash}");

                var transactionReceipt = await _policy.ExecuteAsync(async () =>
                {
                    var result = await _blockChainService.GetTransactionReceiptByTransactionID(
                        transactionHash, CommonConstants.Url);

                    if (result == null)
                    {
                        _logger.LogInformation("retry get receipt of transaction hash: {0}", transactionHash);

                        throw new ArgumentNullException($"Cannot GetTransactionReceipt By {transactionHash}");
                    }

                    return result;
                });

                var transaction = await _blockChainService.GetTransactionByTransactionID(
                    transactionHash, CommonConstants.Url);

                var uft8Convertor = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();

                var transactionId = uft8Convertor.ConvertFromHex(transaction.Input);

                var saleDefi = _saleDefiRepository.FindById(Guid.Parse(transactionId));

                var balance = Web3.Convert.FromWei(transaction.Value);

                if (!transactionReceipt.Succeeded(true))
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"TransactionReceipt's status was failed: {transactionReceipt.Status.Value}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }


                //compare dapp transaction with blockchain transaction
                if (saleDefi.TransactionState != SaleDefiTransactionState.Requested)
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {saleDefi.TransactionState}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"MetaMaskState was not matched: {saleDefi.TransactionState}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare transaction with blockchain transaction
                if (saleDefi.AddressFrom.ToLower() != transaction.From.ToLower()
                    || saleDefi.AddressTo.ToLower() != transaction.To.ToLower())
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: Transaction's infor was not matched";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: Transaction's infor was not matched: ", transaction);

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare amount
                if (balance != saleDefi.BNBAmount)
                {
                    saleDefi.DateUpdated = DateTime.Now;
                    saleDefi.TransactionState = SaleDefiTransactionState.Failed;
                    saleDefi.BNBTransactionHash = transactionHash;
                    saleDefi.Remarks = $"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}";

                    _unitOfWork.Commit();

                    _logger.LogError($"VerifyMetaMaskRequest: " +
                        $"Transaction's balance was not matched: {balance}");

                    return new GenericResult(false,
                        "Your transction was invalid. Please Contact administrator for support!");
                }

                saleDefi.TransactionState = SaleDefiTransactionState.Confirmed;
                saleDefi.BNBTransactionHash = transactionHash;
                saleDefi.DateUpdated = DateTime.Now;

                _unitOfWork.Commit();

                #region Transfer token
                var balanceTransfer = Math.Round(saleDefi.TokenAmount, 2);

                var tranansaction = await _blockChainService.SendERC20Async(
                    CommonConstants.TransferPrKey, saleDefi.AddressFrom,
                           CommonConstants.TokenContract, balanceTransfer,
                           CommonConstants.TokenDecimals, CommonConstants.Url);

                if (tranansaction.Succeeded(true))
                {
                    saleDefi.TokenTransactionHash = tranansaction.TransactionHash;

                    _unitOfWork.Commit();

                    #region Add wallet txn deposit token

                    _walletTransactionService.Add(new WalletTransactionViewModel
                    {
                        AddressFrom = CommonConstants.TransferPuKey,
                        AddressTo = saleDefi.AddressFrom,
                        Amount = balanceTransfer,
                        AmountReceive = balanceTransfer,
                        DateCreated = DateTime.Now,
                        TransactionHash = tranansaction.TransactionHash,
                        Fee = 0,
                        FeeAmount = 0,
                        Type = WalletTransactionType.BuyToken,
                        Unit = Unit.Token,
                        Remarks = JsonConvert.SerializeObject(saleDefi)
                    });
                    _unitOfWork.Commit();

                    //send chat bot
                    var depositMessage = TelegramBotHelper
                        .BuildReportDepositSaleRoundMessage(
                        new DeFiMessageParam
                        {
                            Title = "Buy Token Defi",
                            AmountBNB = balance,
                            DepositAt = DateTime.Now,
                            AmountToken = saleDefi.TokenAmount,
                            UserWallet = saleDefi.AddressFrom,
                            SystemWallet = saleDefi.AddressTo,
                            Email = string.Empty,
                        });

                    await _botTelegramService.SendMessageAsyncWithSendingBalance(
                        TelegramBotActionType.Deposit, depositMessage, TelegramBotHelper.DepositGroup);
                    #endregion


                }
                #endregion


                _logger.LogInformation($"End call VerifyMetaMaskRequest with transaction hash:");

                return new GenericResult(true, $"Successed to buy {CommonConstants.TOKEN_CODE}");
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    //var metamaskTransaction = _saleDefiRepository.FindById(Guid.Parse(tempDappTransaction));
                    //metamaskTransaction.Remarks = e.Message;
                    //metamaskTransaction.BNBTransactionHash = transactionHash;
                    //metamaskTransaction.TransactionState = SaleDefiTransactionState.Failed;

                    //_unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return new GenericResult(false, e.Message);
            }
        }

        public decimal CalculateTokenAmount(decimal bnbAmount, decimal priceBNBBep20)
        {
            decimal priceToken = _configService.GetTokenPrice();

            var amountUSD = Math.Round(bnbAmount * priceBNBBep20, 2);

            return Math.Round(amountUSD / priceToken, 2);
        }

        public string GetLatestTransactionByWalletAddress(string dappTxnHash)
        {
            var txnId = dappTxnHash.HexToUTF8String();

            var guidId = Guid.Parse(txnId);

            var query = _saleDefiRepository.FindAll(x => x.TransactionState == SaleDefiTransactionState.Requested
            && x.Id == guidId);

            query = query.OrderByDescending(x => x.DateCreated);

            var txn = query.FirstOrDefault();

            if (txn == null)
                return string.Empty;

            return txn.Id.ToString();
        }

        #endregion

        #region Sale Manual
        public async Task<GenericResult> ProcessBuyToken(SaleManualViewModel model, string userId)
        {
            try
            {
                var appUser = await _userManager.FindByIdAsync(userId);

                decimal totalUSDBuy = 0;

                switch (model.Unit)
                {
                    case Unit.USDT:

                        if (appUser.USDTAmount < model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        if (model.Amount < CommonConstants.MinBuyToken)
                            return new GenericResult(false, $"Min buy is ${CommonConstants.MinBuyToken}");

                        totalUSDBuy = model.Amount;

                        appUser.USDTAmount -= model.Amount;

                        break;
                    case Unit.BUSD:

                        if (appUser.BUSDAmount < model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        if (model.Amount < CommonConstants.MinBuyToken)
                            return new GenericResult(false, $"Min buy is ${CommonConstants.MinBuyToken}");

                        totalUSDBuy = model.Amount;

                        appUser.BUSDAmount -= model.Amount;

                        break;
                    case Unit.BNB:

                        if (appUser.BNBAmount < model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        var coinPrice = _blockChainService.GetCurrentPrice("BNB", "USD");
                        if (coinPrice <= 0)
                            return new GenericResult(false, "Payment failed, retry later");

                        totalUSDBuy = model.Amount * coinPrice;

                        if (totalUSDBuy < CommonConstants.MinBuyToken)
                            return new GenericResult(false, $"Min buy is ${CommonConstants.MinBuyToken}");

                        appUser.BNBAmount -= model.Amount;

                        break;
                    default:
                        return new GenericResult(false, "Invalid payment");
                }

                var tokenPrice = _configService.GetTokenPrice();

                if (tokenPrice <= 0)
                    return new GenericResult(false, "Invalid payment");

                var receivedToken = totalUSDBuy / tokenPrice;

                appUser.BCAmount += receivedToken;

                var result = await _userManager.UpdateAsync(appUser);

                if (!result.Succeeded)
                    return new GenericResult(false, "Invalid payment");

                #region Save Txn

                var txnHash = Guid.NewGuid().ToString("N");

                _walletTransactionService.AddTransaction(
                    appUser.Id, model.Amount, model.Amount,
                    WalletTransactionType.BuyToken,
                    $"Wallet {model.Unit.GetDescription()}", "System",
                    model.Unit, 0, 0, txnHash);

                _walletTransactionService.AddTransaction(
                    appUser.Id, receivedToken, receivedToken,
                    WalletTransactionType.BuyToken,
                    "System", $"Wallet {Unit.Token.GetDescription()}",
                    Unit.Token, 0, 0, txnHash);
                #endregion

                await SaleAffiliate(totalUSDBuy, model.Amount, model.Unit, appUser);

                return new GenericResult(true, "Payment buy token is successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError("SaleService_ProcessBuyToken: {0}", ex.Message);
                return new GenericResult(false, ex.Message);
            }
        }

        async Task<bool> SaleAffiliate(
            decimal totalUSDBuy, decimal amount, Unit unit, AppUser appUser)
        {
            if (appUser.ReferralId == null)
                return false;


            var txnHash = Guid.NewGuid().ToString("N");


            var f1 = await _userManager.FindByIdAsync(appUser.ReferralId.Value.ToString());
            if (f1 == null || f1.IsSystem)
                return false;

            await AddSaleAffiliate(f1, appUser, amount, unit, totalUSDBuy, 15, 1, txnHash);


            if (f1.ReferralId == null)
                return false;


            var f2 = await _userManager.FindByIdAsync(f1.ReferralId.Value.ToString());
            if (f2 == null || f2.IsSystem)
                return false;

            await AddSaleAffiliate(f2, appUser, amount, unit, totalUSDBuy, 10, 2, txnHash);


            if (f2.ReferralId == null )
                return false;

            var f3 = await _userManager.FindByIdAsync(f2.ReferralId.Value.ToString());

            if (f3 == null || f3.IsSystem)
                return false;

            await AddSaleAffiliate(f3, appUser, amount, unit, totalUSDBuy, 5, 3, txnHash);

            return true;
        }

        private async Task AddSaleAffiliate(
            AppUser fn,
            AppUser appUser,
            decimal amount,
            Unit unit,
            decimal totalUSDBuy,
            decimal percentAffiliate,
            int referralLevel, string txnHash)
        {
            //if (fn.IsAffiliateInsurance && fn.InvestAmount > 0)
            //{
            var profitBonus = amount * (percentAffiliate / 100);

            switch (unit)
            {
                case Unit.USDT:
                    fn.USDTAmount += profitBonus;
                    break;
                case Unit.BUSD:
                    fn.BUSDAmount += profitBonus;
                    break;
                case Unit.BNB:
                    fn.BNBAmount += profitBonus;
                    break;
            }

            var ret = await _userManager.UpdateAsync(fn);

            if (ret.Succeeded)
            {
                _saleAffiliateRepository.Add(new SaleAffiliate
                {
                    AppUserId = fn.Id,
                    FromSponsor = appUser.Sponsor,
                    FromAppUserId = appUser.Id,
                    DateCreated = DateTime.Now,
                    InterestRate = percentAffiliate,
                    ProfitAmount = profitBonus,
                    ReferralLevel = referralLevel,
                    USDAmount = totalUSDBuy,
                    Amount = amount,
                    Unit = unit
                });

                _unitOfWork.Commit();

                _walletTransactionService.AddTransaction(fn.Id,
                profitBonus, profitBonus,
                WalletTransactionType.SaleAffiliate,
                "System", $"Wallet {unit.GetDescription()}",
                unit, 0, 0,
                txnHash,
                $"Sale affiliate F{referralLevel} {percentAffiliate}% of {totalUSDBuy} from {appUser.Email} = " +
                $"{profitBonus} {unit.GetDescription()} ");
            }
            //}
        }
        #endregion
    }
}

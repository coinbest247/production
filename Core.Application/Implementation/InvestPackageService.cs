using Core.Application.Interfaces;
using Core.Application.ViewModels.AffiliateAgent;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.InvestPacakage;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.Telegram;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class InvestPackageService : IInvestPackageService
    {
        private readonly IInvestPackageRepository _investPackageRepository;
        private readonly IInvestPackageRewardRepository _investPackageRewardRepository;
        private readonly IInvestProfitHistoryRepository _investProfitHistoryRepository;
        private readonly IInvestPackageAffiliateRepository _investPackageAffiliateRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ILogger<InvestPackageService> _logger;
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly IConfigService _configService;

        public InvestPackageService(
            IInvestPackageRepository investPackageRepository,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IBlockChainService blockChainService,
            IWalletTransactionService walletTransactionService,
            ILogger<InvestPackageService> logger,
            TelegramBotWrapper botTelegramService,
            IConfigService configService,
            IInvestPackageRewardRepository investPackageRewardRepository,
            IInvestPackageAffiliateRepository investPackageAffiliateRepository,
            IInvestProfitHistoryRepository investProfitHistoryRepository)
        {
            _investProfitHistoryRepository = investProfitHistoryRepository;
            _investPackageAffiliateRepository = investPackageAffiliateRepository;
            _configService = configService;
            _investPackageRepository = investPackageRepository;
            _logger = logger;
            _investPackageRepository = investPackageRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _walletTransactionService = walletTransactionService;
            _botTelegramService = botTelegramService;
            _investPackageRewardRepository = investPackageRewardRepository;
        }

        public PagedResult<InvestPackageViewModel> GetAllPaging(string keyword,
            Guid appUserId,
            int type,
            int pageIndex,
            int pageSize)
        {
            var query = _investPackageRepository.FindAll();

            //if (!string.IsNullOrWhiteSpace(keyword))
            //    query = query.Where(x => x.Name.ToLower().Contains(keyword.ToLower()));

            if (type > 0)
            {
                query = query.Where(x => x.Type == (InvestPackageType)type);
            }

            query = query.Where(x => x.AppUserId == appUserId);

            var totalRow = query.Count();
            var data = query.OrderBy(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new InvestPackageViewModel
                {
                    Id = x.Id,
                    Remarks = x.Remarks,
                    AppUserId = x.AppUserId,
                    CancelOn = x.CancelOn,
                    CompletedOn = x.CompletedOn,
                    DateCreated = x.DateCreated,
                    DateUpdated = x.DateUpdated,
                    InvestAmount = x.InvestAmount,
                    LastReceived = x.LastReceived,
                    ReceivedCount = x.ReceivedCount,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    UnitName = x.Unit.GetDescription(),
                    USDAmount = x.USDAmount,
                }).ToList();

            return new PagedResult<InvestPackageViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public PagedResult<InvestPackageAffiliateViewModel> GetAllInvestPackageAffiliatePaging(string keyword,
            Guid appUserId,
            int pageIndex,
            int pageSize)
        {
            var query = _investPackageAffiliateRepository.FindAll();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.FromSponsor.ToLower().Contains(keyword.ToLower()));

            query = query.Where(x => x.AppUserId == appUserId);

            var totalRow = query.Count();
            var data = query.OrderBy(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new InvestPackageAffiliateViewModel
                {
                    Id = x.Id,
                    ReferralLevel = x.ReferralLevel,
                    ProfitAmount = x.ProfitAmount,
                    InterestRate = x.InterestRate,
                    DateCreated = x.DateCreated,

                    FromSponsor = x.FromSponsor,
                    InvestAmountInUSDT = x.InvestAmountInUSDT,

                }).ToList();

            return new PagedResult<InvestPackageAffiliateViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }


        public InvestPackageViewModel GetById(int id)
        {
            var model = _investPackageRepository.FindById(id);
            if (model == null)
                return null;

            return new InvestPackageViewModel
            {
                Id = model.Id,
                Remarks = model.Remarks,
                AppUserId = model.AppUserId,
                CancelOn = model.CancelOn,
                CompletedOn = model.CompletedOn,
                DateCreated = model.DateCreated,
                DateUpdated = model.DateUpdated,
                InvestAmount = model.InvestAmount,
                LastReceived = model.LastReceived,
                ReceivedCount = model.ReceivedCount,
                Type = model.Type,
                TypeName = model.Type.GetDescription()
            };
        }

        public void Insert(InvestPackageViewModel investPackageVm)
        {
            var model = new InvestPackage
            {
                AppUserId = investPackageVm.AppUserId,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                InvestAmount = investPackageVm.InvestAmount,
                ReceivedCount = investPackageVm.ReceivedCount,
                Type = investPackageVm.Type,
                Unit = investPackageVm.Unit,
                USDAmount = investPackageVm.USDAmount,
                Remarks = investPackageVm.Remarks
            };

            _investPackageRepository.Add(model);

            _unitOfWork.Commit();
        }

        public async Task<GenericResult> ProcessBuyInvestPackage(PaymentInvestModel model, string userId)
        {
            try
            {
                //if (model.Amount < CommonConstants.MinInvestBotTrade)
                //    return new GenericResult(false,
                //        $"Invalid invest amount. Minimum ${CommonConstants.MinInvestBotTrade}");

                //if (model.Amount > CommonConstants.MaxInvestBotTrade)
                //    return new GenericResult(false, 
                //        $"Invalid invest amount. Maximum ${CommonConstants.MaxInvestBotTrade}");

                if (model.Amount <= 0)
                    return new GenericResult(false, $"Invalid invest amount.");

                var appUser = await _userManager.FindByIdAsync(userId);

                decimal totalInUSDT = 0;

                switch (model.Unit)
                {
                    case Unit.USDT:
                        if (model.Amount < CommonConstants.MinInvestBotTrade)
                            return new GenericResult(false,
                                $"Invalid invest amount. Minimum ${CommonConstants.MinInvestBotTrade}");

                        if (appUser.USDTAmount < model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        totalInUSDT = model.Amount;

                        appUser.USDTAmount -= model.Amount;

                        break;
                    case Unit.BUSD:
                        if (model.Amount < CommonConstants.MinInvestBotTrade)
                            return new GenericResult(false,
                                $"Invalid invest amount. Minimum ${CommonConstants.MinInvestBotTrade}");

                        if (appUser.BUSDAmount < model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        totalInUSDT = model.Amount;

                        appUser.BUSDAmount -= model.Amount;

                        break;
                    case Unit.BNB:
                        if (appUser.BNBAmount < model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        var priceUSD = _blockChainService.GetCurrentPrice("BNB", "USD");
                        if (priceUSD <= 0)
                            return new GenericResult(false, "Payment failed, retry later");

                        var bnbRequired = Math.Round(CommonConstants.MinInvestBotTrade / priceUSD, 4);
                        if (model.Amount < bnbRequired)
                            return new GenericResult(false,
                                $"Invalid invest amount. Minimum {bnbRequired} BNB");

                        totalInUSDT = priceUSD * model.Amount;

                        appUser.BNBAmount -= model.Amount;

                        break;
                    default:
                        return new GenericResult(false, "Invalid payment");
                }



                appUser.InvestBotAmount += totalInUSDT;

                var result = await _userManager.UpdateAsync(appUser);

                if (!result.Succeeded)
                    return new GenericResult(false, "Invalid payment");

                #region Save Txn

                var txnHash = Guid.NewGuid().ToString("N");

                _walletTransactionService.AddTransaction(
                    appUser.Id,
                    model.Amount,
                    model.Amount,
                    WalletTransactionType.InvestBot,
                    $"Wallet {model.Unit.GetDescription()}",
                    "SYSTEM",
                    model.Unit,
                    0,
                    0,
                    txnHash,
                    "Payment invest from " + appUser.Sponsor + " by " + model.Amount + " " + model.Unit.GetDescription());

                Insert(new InvestPackageViewModel
                {
                    InvestAmount = model.Amount,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    AppUserId = appUser.Id,
                    Type = InvestPackageType.Active,
                    ReceivedCount = 0,
                    Remarks = "Payment invest from " + appUser.Sponsor + " by " + model.Amount + " " + model.Unit.GetDescription(),
                    USDAmount = totalInUSDT,
                    Unit = model.Unit
                });

                #endregion

                await SendInvestmentTeleGroup(model.Amount, model.Unit, appUser);

                return new GenericResult(true, "Payment invest is successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError("StakingController_BuyStaking: {0}", ex.Message);
                return new GenericResult(false, ex.Message);
            }
        }

        public async Task SendInvestmentTeleGroup(
            decimal investAmount, Unit unit, AppUser appUser)
        {
            var sponsorEmail = string.Empty;

            if (appUser.ReferralId != null)
            {
                var tmp = await _userManager.FindByIdAsync(appUser.ReferralId.ToString());
                if (tmp != null)
                {
                    sponsorEmail = tmp.Email;
                }
            }
            var message = TelegramBotHelper.BuildReportInvestmentMessage
                (new DepositMessageParam
                {
                    Amount = investAmount,
                    CreatedDate = DateTime.Now,
                    Currency = $"{unit.GetDescription()}",
                    Email = appUser.Email,
                    SponsorEmail = sponsorEmail,
                    WalletFrom = $"Wallet {unit.GetDescription()}",
                    WalletTo = "SYSTEM",
                }
           );

            await _botTelegramService.SendMessageAsyncWithSendingBalance(TelegramBotActionType.Deposit, message, TelegramBotHelper.DepositGroup);

        }

        public async Task<GenericResult> ProcessCancelInvestPackage(PaymentInvestModel model, string userId)
        {
            try
            {

                var appUser = await _userManager.FindByIdAsync(userId);

                var investPackage = _investPackageRepository.FindById(model.Id);

                if (investPackage == null)
                    return new GenericResult(false, "Package invalid");

                if (investPackage.AppUserId != appUser.Id || investPackage.Type != InvestPackageType.Active)
                    return new GenericResult(false, "Package invalid");

                int investDate = (DateTime.Now - investPackage.DateCreated).Days;

                if (investDate < 31)
                    return new GenericResult(false, "Min invest day is 31 to cancel invest package");

                decimal refundRate = 0;

                if (investDate < 181)
                    refundRate = 0.5m;
                if (investDate >= 181 && investDate <= 290)
                    refundRate = 0.85m;
                if (investDate > 290)
                    refundRate = 1;

                var refundAmount = refundRate * investPackage.USDAmount;

                appUser.USDTAmount += refundAmount;

                var result = await _userManager.UpdateAsync(appUser);
                if (!result.Succeeded)
                    return new GenericResult(false, "Cancel fail, retry later");

                investPackage.CancelOn = DateTime.Now;
                investPackage.DateUpdated = DateTime.Now;
                investPackage.Type = InvestPackageType.Cancel;
                _investPackageRepository.Update(investPackage);
                _unitOfWork.Commit();

                var txnHash = Guid.NewGuid().ToString("N");

                _walletTransactionService.AddTransaction(
                    appUser.Id,
                    refundAmount,
                    refundAmount,
                    WalletTransactionType.CancelBot,
                    "SYSTEM",
                    $"Wallet {Unit.USDT.GetDescription()}",
                    Unit.USDT,
                    0,
                    0,
                    txnHash,
                    "Cancel invest from " + appUser.Sponsor + " and refund " + refundAmount + " " + model.Unit.GetDescription() + $" for invested {investDate} days");

                return new GenericResult(true, "Cancel invest is successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError("StakingController_CancelInvest: {0}", ex.Message);
                return new GenericResult(false, ex.Message);
            }
        }

        public async Task<bool> ProcessDailyInvestProfit()
        {
            try
            {
                var queryStakings = _investPackageRepository.FindAll(x =>
                        x.Type == InvestPackageType.Active
                        && x.AppUser.IsShowOff == false
                        && x.ReceivedCount < 360
                        && x.DateCreated.Date < DateTime.Now.Date
                        && (x.LastReceived == null || x.LastReceived.Value.Date < DateTime.Now.Date))
                    .ToList();

                var yesterdayProfit = GetYesterdayInvestProfit();

                foreach (var investPackage in queryStakings)
                {
                    var appUser = await _userManager.FindByIdAsync(investPackage.AppUserId.ToString());

                    if (appUser == null)
                        continue;

                    var packageProfit = Math.Round((yesterdayProfit / 100 * investPackage.USDAmount) / 30, 2);

                    var txnHash = Guid.NewGuid().ToString("N");

                    appUser.BotTradeAmount += packageProfit;

                    var result = await _userManager.UpdateAsync(appUser);

                    if (!result.Succeeded)
                    {
                        //Please write log when has exception
                        _logger.LogError($"ProcessDailyInvestProfit: AppUser {appUser.Email}" +
                            $" ,investPackageId: {investPackage.Id} " +
                            $" ,packageProfit: {packageProfit} ");

                        continue;
                    }

                    investPackage.LastReceived = DateTime.Now;
                    investPackage.ReceivedCount += 1;
                    investPackage.DateUpdated = DateTime.Now;
                    _investPackageRepository.Update(investPackage);

                    _walletTransactionService.AddTransaction(appUser.Id,
                    packageProfit,
                    packageProfit,
                    WalletTransactionType.ProfitBot,
                    "SYSTEM",
                    $"Wallet BotTrade",
                    Unit.USDT,
                    0,
                    0,
                    txnHash,
                    $"Payment invest profit to {appUser.Sponsor} by " +
                    $"{packageProfit} {Unit.USDT.GetDescription()} from Invest " +
                    $"{investPackage.InvestAmount} " +
                    $"{investPackage.Unit.GetDescription()}");

                    SaveInvestPackageReward(appUser, investPackage, yesterdayProfit, packageProfit);

                    await CompletedInvestPackageCheck(investPackage, appUser);

                    await PaymentCommissionOnProfit(packageProfit, investPackage.USDAmount, appUser);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ProcessDailyInvestProfit: Exception {ex.Message} {ex.StackTrace}");
            }

            return true;
        }

        private void AddPackageAffiliate(InvestPackageAffiliate affiliateReport)
        {
            _investPackageAffiliateRepository.Add(affiliateReport);

            _unitOfWork.Commit();
        }

        async Task<bool> CompletedInvestPackageCheck(InvestPackage investPackage, AppUser appUser)
        {
            if (investPackage.ReceivedCount < 360)
                return false;


            investPackage.CompletedOn = DateTime.Now;
            investPackage.DateUpdated = DateTime.Now;
            investPackage.Type = InvestPackageType.Completed;
            investPackage.Remarks += $" Completed invest, refund 100% invest amount and bonus 100% invest amount";

            _investPackageRepository.Update(investPackage);


            decimal totalReceive = investPackage.USDAmount * 2;

            appUser.USDTAmount += totalReceive;

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                _logger.LogError($"CompletedInvestPackageCheck: AppUser {appUser.Email}" +
                            $" ,investPackageId: {investPackage.Id} " +
                            $" ,USDTAmount: {totalReceive} ");

                //Please write log when has exception

                return false;
            }

            var txnHash = Guid.NewGuid().ToString("N");

            _walletTransactionService.AddTransaction(appUser.Id,
                investPackage.USDAmount,
                investPackage.USDAmount,
                WalletTransactionType.CompletedBot,
                "SYSTEM",
                $"Wallet {Unit.USDT.GetDescription()}",
                Unit.USDT,
                0,
                0,
                txnHash,
                $"Refund invest amount to {appUser.Sponsor} by " +
                $"{investPackage.USDAmount} {Unit.USDT.GetDescription()} from Completed Invest " +
                $"{investPackage.InvestAmount} " +
                $"{investPackage.Unit.GetDescription()}");

            _walletTransactionService.AddTransaction(appUser.Id,
                investPackage.USDAmount,
                investPackage.USDAmount,
                WalletTransactionType.BonusCompletedBot,
                "SYSTEM",
                $"Wallet {Unit.USDT.GetDescription()}",
                Unit.USDT,
                0,
                0,
                txnHash,
                $"Bonus 100% invest amount to {appUser.Sponsor} by " +
                $"{investPackage.USDAmount} {Unit.USDT.GetDescription()} from Completed Invest " +
                $"{investPackage.InvestAmount} " +
                $"{investPackage.Unit.GetDescription()}");

            return true;
        }

        async Task<bool> PaymentCommissionOnProfit(
            decimal profitAmount, decimal investAmountInUSDT, AppUser appUser)
        {
            if (appUser.ReferralId == null)
                return false;

            var txnHash = Guid.NewGuid().ToString("N");


            var f1 = await _userManager.FindByIdAsync(appUser.ReferralId.Value.ToString());

            if (f1 == null || f1.IsSystem)
                return false;

            if (f1.IsAffiliateAgent && f1.InvestBotAmount > 0)
            {
                var profitBonus = profitAmount * 0.3m;

                f1.BotTradeAmount += profitBonus;

                var ret = await _userManager.UpdateAsync(f1);
                if (ret.Succeeded)
                {
                    #region Save Referral Report Info
                    AddPackageAffiliate(new InvestPackageAffiliate
                    {
                        AppUserId = f1.Id,
                        FromSponsor = appUser.Sponsor,
                        FromAppUserId = appUser.Id,
                        DateCreated = DateTime.Now,
                        InterestRate = 30,
                        ProfitAmount = profitBonus,
                        ReferralLevel = 1,
                        InvestAmountInUSDT = investAmountInUSDT
                    });

                    #endregion

                    _walletTransactionService.AddTransaction(f1.Id,
                    profitBonus,
                    profitBonus,
                    WalletTransactionType.CommissionOnProfitBot,
                    "SYSTEM",
                    $"Wallet BotTrade",
                    Unit.USDT,
                    0,
                    0,
                    txnHash,
                    $"Invest profit referral F1 30% of {profitAmount} from {appUser.Sponsor} = " +
                    $"{profitBonus} {Unit.USDT.GetDescription()} ");
                }
            }

            if (f1.ReferralId == null)
                return false;

            var f2 = await _userManager.FindByIdAsync(f1.ReferralId.Value.ToString());

            if (f2 == null || f2.IsSystem)
                return false;

            if (f2.IsAffiliateAgent && f2.InvestBotAmount > 0)
            {
                var profitBonus = profitAmount * 0.1m;

                f2.BotTradeAmount += profitBonus;

                var ret = await _userManager.UpdateAsync(f2);
                if (ret.Succeeded)
                {
                    #region Save Referral Report Info
                    AddPackageAffiliate(new InvestPackageAffiliate
                    {
                        AppUserId = f2.Id,
                        FromSponsor = appUser.Sponsor,
                        FromAppUserId = appUser.Id,
                        DateCreated = DateTime.Now,
                        InterestRate = 10,
                        ProfitAmount = profitBonus,
                        ReferralLevel = 2,
                        InvestAmountInUSDT = investAmountInUSDT
                    });
                    #endregion

                    _walletTransactionService.AddTransaction(f2.Id,
                    profitBonus,
                    profitBonus,
                    WalletTransactionType.CommissionOnProfitBot,
                    "SYSTEM",
                    $"Wallet BotTrade",
                    Unit.USDT,
                    0,
                    0,
                    txnHash,
                    $"Invest profit referral F2 10% of {profitAmount} from {appUser.Sponsor} = " +
                    $"{profitBonus} {Unit.USDT.GetDescription()} ");
                }
            }

            if (f2.ReferralId == null)
                return false;

            var f3 = await _userManager.FindByIdAsync(f2.ReferralId.Value.ToString());

            if (f3 == null || f3.IsSystem)
                return false;

            if (f3.IsAffiliateAgent && f3.InvestBotAmount > 0)
            {
                var profitBonus = profitAmount * 0.1m;

                f3.BotTradeAmount += profitBonus;

                var ret = await _userManager.UpdateAsync(f3);
                if (ret.Succeeded)
                {
                    #region Save Referral Report Info
                    AddPackageAffiliate(new InvestPackageAffiliate
                    {
                        AppUserId = f3.Id,
                        FromSponsor = appUser.Sponsor,
                        FromAppUserId = appUser.Id,
                        DateCreated = DateTime.Now,
                        InterestRate = 10,
                        ProfitAmount = profitBonus,
                        ReferralLevel = 3,
                        InvestAmountInUSDT = investAmountInUSDT
                    });
                    #endregion

                    _walletTransactionService.AddTransaction(f3.Id,
                    profitBonus,
                    profitBonus,
                    WalletTransactionType.CommissionOnProfitBot,
                    "SYSTEM",
                    $"Wallet BotTrade",
                    Unit.USDT,
                    0,
                    0,
                    txnHash,
                    $"Invest profit referral F3 10% of {profitAmount} from {appUser.Sponsor} = " +
                    $"{profitBonus} {Unit.USDT.GetDescription()} ");
                }
            }

            return true;
        }

        decimal GetYesterdayInvestProfit()
        {
            var yesterdayDate = DateTime.Now.Date.AddDays(-1);

            var query = _investProfitHistoryRepository
                                        .FindAll(x => x.DateCreated.Date == yesterdayDate);

            if (!query.Any())
            {
                return 7;
            }

            var winAmount = query.Where(x => x.IsWin).Sum(d => d.ProfitPercent);

            var loseAmount = query.Where(x => !x.IsWin).Sum(d => d.ProfitPercent);

            return winAmount - loseAmount;
        }

        void SaveInvestPackageReward(AppUser appUser, InvestPackage investPackage, decimal interestRate, decimal amount)
        {
            _investPackageRewardRepository.Add(new InvestPackageReward
            {
                Amount = amount,
                InterestRate = interestRate,
                AppUserId = appUser.Id,
                DateCreated = DateTime.Now,
                InvestAmount = investPackage.USDAmount,
                InvestId = investPackage.Id,
                Sponsor = appUser.Sponsor
            });

            _unitOfWork.Commit();
        }

        public decimal GetTotalInvestBotTrade(List<Guid> appUserIds)
        {
            var query = _investPackageRepository.FindAll();

            query = query.Where(x => appUserIds.Contains(x.AppUserId));

            return query.Sum(x => x.USDAmount);
        }
    }
}

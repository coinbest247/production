using Core.Application.Interfaces;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.InvestPacakage;
using Core.Application.ViewModels.Staking;
using Core.Application.ViewModels.System;
using Core.Data.EF.Repositories;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.Telegram;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class StakingService : IStakingService
    {
        private readonly IStakingRepository _stakingRepository;
        private readonly IStakingAffiliateRepository _stakingAffiliateRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StakingService> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IBlockChainService _blockChainService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IConfigService _configService;
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly IStakingRewardRepository _stakingRewardRepository;

        public StakingService(
            IConfigService configService,
            IBlockChainService blockChainService,
            IConfiguration configuration,
            IStakingAffiliateService stakingAffiliateService,
            UserManager<AppUser> userManager,
            ILogger<StakingService> logger,
            IStakingRepository stakingRepository,
            IWalletTransactionService walletTransactionService,
            IUnitOfWork unitOfWork,
            TelegramBotWrapper botTelegramService,
            IInvestPackageService investPackageService,
            IStakingRewardRepository stakingRewardRepository,
            IStakingAffiliateRepository stakingAffiliateRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
            _configService = configService;
            _stakingRepository = stakingRepository;
            _blockChainService = blockChainService;
            _walletTransactionService = walletTransactionService;
            _botTelegramService = botTelegramService;
            _stakingRewardRepository = stakingRewardRepository;
            _stakingAffiliateRepository = stakingAffiliateRepository;
        }

        public PagedResult<StakingViewModel> GetAllPaging(string keyword, string appUserId,
            DateTime? fromDate,
            DateTime? toDate,
            int pageIndex,
            int pageSize)
        {
            var query = _stakingRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                x.AppUser.Email.Contains(keyword)
                );
            }

            if (fromDate != null && toDate != null)
                query = query.Where(x => x.DateCreated >= fromDate.Value && x.DateCreated <= toDate.Value);

            

            if (!string.IsNullOrWhiteSpace(appUserId))
                query = query.Where(x => x.AppUserId.ToString() == appUserId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new StakingViewModel()
                {
                    Id = x.Id,
                    
                    InterestRate = x.InterestRate,
                    ReceiveAmount = x.ReceiveAmount,
                    ReceiveLatest = x.ReceiveLatest,
                    ReceiveTimes = x.ReceiveTimes,
                    StakingAmount = x.StakingAmount,
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    Sponsor = x.AppUser.Sponsor,
                    DateCreated = x.DateCreated,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    CancelOn = x.CancelOn,
                    CompletedOn = x.CompletedOn,
                    PaymentUnit = x.PaymentUnit.GetDescription(),
                    PaymentAmount = x.PaymentAmount
                    
                }).ToList();

            return new PagedResult<StakingViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public PagedResult<StakingRewardViewModel> GetAllProfitPaging(string keyword, string appUserId,
            DateTime? fromDate,
            DateTime? toDate,
            int pageIndex,
            int pageSize)
        {
            var query = _stakingRewardRepository.FindAll(x=>x.AppUser,x=>x.Staking);

            
            if (fromDate != null && toDate != null)
                query = query.Where(x => x.DateCreated >= fromDate.Value && x.DateCreated <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(appUserId))
                query = query.Where(x => x.AppUserId.ToString() == appUserId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new StakingRewardViewModel()
                {
                    Id = x.Id,
                    AppUserName = x.AppUser.UserName,
                    Sponsor = x.AppUser.Sponsor,
                    StakingAmount = x.Staking.StakingAmount,
                    InterestRate = x.InterestRate,
                    Amount = x.Amount,
                    DateCreated = x.DateCreated,
                    

                }).ToList();

            return new PagedResult<StakingRewardViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public List<StakingViewModel> GetLeaderboard(int top)
        {
            var query = _stakingRepository.FindAll(x => x.AppUser);


            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Take(top)
                .Select(x => new StakingViewModel()
                {
                    Id = x.Id,
                    
                    InterestRate = x.InterestRate,
                    ReceiveAmount = x.ReceiveAmount,
                    ReceiveLatest = x.ReceiveLatest,
                    ReceiveTimes = x.ReceiveTimes,
                    StakingAmount = x.StakingAmount,
                    
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    Sponsor = x.AppUser.Sponsor,
                    DateCreated = x.DateCreated,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription()
                }).ToList();

            return data;
        }

        public IQueryable<Staking> GetAllByUserId(Guid appUserId)
        {
            var query = _stakingRepository.FindAll(x => x.AppUserId == appUserId);
            return query;
        }

        public decimal GetTotalPackage(Guid userId, StakingType? type)
        {
            decimal totalStaking = 0;
            var staking = _stakingRepository.FindAll(x => x.AppUserId == userId);

            if (type != null)
                staking = staking.Where(x => x.Type == type);

            totalStaking = staking.Sum(x => x.StakingAmount);

            return totalStaking;
        }

        public async Task<GenericResult> BuyInvestPackage(string modelJson, string userId)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<PaymentInvestModel>(modelJson);

                
                if (model.Amount < 100 )
                    return new GenericResult(false, "Invalid invest amount. Minimum 100$");

                var appUser = await _userManager.FindByIdAsync(userId);

                if (!appUser.IsAffiliateAgent)
                    return new GenericResult(false, "Must buy invest insurance before invest");

                switch (model.Unit)
                {
                    case Unit.USDT:
                        if (appUser.USDTAmount<model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        appUser.USDTAmount -= model.Amount; 

                        break;
                    case Unit.BUSD:
                        if (appUser.BUSDAmount < model.Amount)
                            return new GenericResult(false, "Balance not enough to invest");

                        appUser.BUSDAmount -= model.Amount;
                        break;
                    case Unit.BNB:
                        var priceUSD = _blockChainService.GetCurrentPrice("BNB","USD");
                        var bnbRequired = model.Amount / priceUSD;
                        if (appUser.BNBAmount < bnbRequired)
                            return new GenericResult(false, "Balance not enough to invest");

                        appUser.BNBAmount -= bnbRequired;
                        break;
                    default:
                        return new GenericResult(false, "Invalid payment");
                }

                var result = await _userManager.UpdateAsync(appUser);

                if (!result.Succeeded)
                    return new GenericResult(false, "Invalid payment");

                await SendChannelTeleGroup(model.Amount,model.Unit, appUser);

                return new GenericResult(true, "Staking is successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError("StakingController_BuyStaking: {0}", ex.Message);
                return new GenericResult(false, ex.Message);
            }
        }

        public async Task<GenericResult> PaymentStakingAsync(BuyStakingViewModel model, Guid userId)
        {
            
            decimal amountPayment = model.AmountPayment;

            var appUser = await _userManager.FindByIdAsync(userId.ToString());

            
            decimal totalInUSDT = 0;

            switch (model.Unit)
            {
                case Unit.USDT:
                    if (appUser.USDTAmount < model.AmountPayment)
                        return new GenericResult(false, "Balance not enough to staking");

                    totalInUSDT = model.AmountPayment;
                    if (totalInUSDT < CommonConstants.MinStakingPI)
                        return new GenericResult(false, $"Min staking is ${CommonConstants.MinStakingPI}");

                    appUser.USDTAmount -= model.AmountPayment;

                    break;
                case Unit.BUSD:
                    if (appUser.BUSDAmount < model.AmountPayment)
                        return new GenericResult(false, "Balance not enough to invest");

                    totalInUSDT = model.AmountPayment;
                    if (totalInUSDT < CommonConstants.MinStakingPI)
                        return new GenericResult(false, $"Min staking is ${CommonConstants.MinStakingPI}");
                    appUser.BUSDAmount -= model.AmountPayment;
                    break;
                case Unit.BNB:
                    var priceUSD = _blockChainService.GetCurrentPrice("BNB", "USD");
                    if (priceUSD <= 0)
                        return new GenericResult(false, "Payment failed, retry later");
                    var bnbRequired = model.AmountPayment / priceUSD;
                    if (appUser.BNBAmount < bnbRequired)
                        return new GenericResult(false, "Balance not enough to invest");

                    totalInUSDT = priceUSD * model.AmountPayment;

                    if (totalInUSDT < CommonConstants.MinStakingPI)
                        return new GenericResult(false, $"Min staking is ${CommonConstants.MinStakingPI}");

                    appUser.BNBAmount -= bnbRequired;
                    break;
                default:
                    return new GenericResult(false, "Invalid payment");
            }

            var piPrice = await _blockChainService.GetTokenPriceByTokenId(16193, "USD");

            var piStakingAmount = totalInUSDT / piPrice;

            piStakingAmount += ( piStakingAmount * 0.2m); // bonus 20% when staking

            appUser.StakingPIAmount += piStakingAmount;

            var updateUserPayment = await _userManager.UpdateAsync(appUser);

            if (updateUserPayment.Succeeded)
            {
                string txt = Guid.NewGuid().ToString();

                _walletTransactionService.AddTransaction(userId,
                    amountPayment,
                    0,
                    WalletTransactionType.Staking,
                    $"Wallet {model.Unit.GetDescription()}",
                    "System",
                    model.Unit,
                    0,
                    0,
                    txt,
                    $"{appUser.Email} Staking {amountPayment} " +
                    $"{model.Unit.GetDescription()} = {piStakingAmount}");

                _stakingRepository.Add(new Staking
                {
                    AppUserId = userId,
                    DateCreated = DateTime.Now,
                    InterestRate = 15,
                    ReceiveAmount = 0,
                    ReceiveTimes = 0,
                    StakingAmount = piStakingAmount,
                    StakingAmountUSDT = amountPayment,
                    Type = StakingType.Process,
                    PaymentAmount = model.AmountPayment,
                    PaymentUnit = model.Unit
                });

                _unitOfWork.Commit();
            }
            else
            {
                return new GenericResult(false,
                    string.Join(",", updateUserPayment.Errors.Select(x => x.Description)));
            }

            return new GenericResult(true, $"Staking successfully");
        }

        
        public StakingViewModel GetById(int id)
        {
            var model = _stakingRepository.FindById(id);
            if (model == null)
                return null;

            var staking = new StakingViewModel()
            {
                Id = model.Id,
                
                InterestRate = model.InterestRate,
                ReceiveAmount = model.ReceiveAmount,
                ReceiveLatest = model.ReceiveLatest,
                ReceiveTimes = model.ReceiveTimes,
                StakingAmount = model.StakingAmount,
                AppUserId = model.AppUserId,
                AppUserName = model.AppUser.UserName,
                Sponsor = model.AppUser.Sponsor,
                DateCreated = model.DateCreated,
                Type = model.Type,
                TypeName = model.Type.GetDescription()
            };

            return staking;
        }

        public void Update(StakingViewModel model)
        {
            var staking = _stakingRepository.FindById(model.Id);

            
            staking.InterestRate = model.InterestRate;
            staking.StakingAmount = model.StakingAmount;
            staking.ReceiveTimes = model.ReceiveTimes;
            staking.ReceiveAmount = model.ReceiveAmount;
            staking.ReceiveLatest = model.ReceiveLatest;
            staking.AppUserId = model.AppUserId;
            staking.DateCreated = model.DateCreated;
            staking.Type = model.Type;
            _stakingRepository.Update(staking);
        }

        public void UpdateTxtHash(int id, string txtHash)
        {
            var staking = _stakingRepository.FindById(id);


            _stakingRepository.Update(staking);
        }

        public Staking Add(StakingViewModel model)
        {
            var staking = new Staking()
            {
                
                InterestRate = model.InterestRate,
                StakingAmount = model.StakingAmount,
                ReceiveTimes = model.ReceiveTimes,
                ReceiveAmount = model.ReceiveAmount,
                ReceiveLatest = model.ReceiveLatest,
                AppUserId = model.AppUserId,
                DateCreated = model.DateCreated,
                Type = model.Type
            };

            _stakingRepository.Add(staking);

            return staking;
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public decimal GetTotalStaking()
        {
            var query = _stakingRepository.FindAll(x => x.AppUser);

            var total = query.Sum(x => x.StakingAmount);

            return total;
        }

        public decimal GetTodayStaking()
        {
            var today = DateTime.UtcNow.Date;

            var query = _stakingRepository.FindAll(x => x.AppUser);

            query = query.Where(x => x.DateCreated >= today);

            var total = query.Sum(x => x.StakingAmount);

            return total;
        }

        public decimal GetMaxout(string appUserId)
        {
            var appUser = _userManager.FindByIdAsync(appUserId).Result;

            decimal totalStaking = 0;

            if (totalStaking == 0)
                return 0;

            var transactions = _walletTransactionService.GetAllByUserId(appUser.Id);

            var profitTransactions = transactions
                 .Where(x => x.Type == WalletTransactionType.StakingLeadership
                 || x.Type == WalletTransactionType.StakingProfit
                 || x.Type == WalletTransactionType.StakingAffiliateOnProfit
                 || x.Type == WalletTransactionType.StakingReferralDirect);

            decimal totalProfit = profitTransactions.Sum(x => x.Amount);


            decimal maxOutProfit = (3 - (totalProfit / totalStaking)) * 100;

            return maxOutProfit;

        }

        //public bool CheckMaxOutProfit(string appUserId)
        //{
        //    var appUser = _userManager.FindByIdAsync(appUserId).Result;

        //    decimal totalStaking = appUser.StakingAmount;

        //    if (totalStaking == 0)
        //        return true;

        //    var transactions = _walletTransactionService.GetAllByUserId(appUser.Id);

        //    var profitTransactions = transactions
        //         .Where(x => x.Type == WalletTransactionType.StakingLeadership
        //         || x.Type == WalletTransactionType.StakingProfit
        //         || x.Type == WalletTransactionType.StakingAffiliateOnProfit
        //         || x.Type == WalletTransactionType.StakingReferralDirect);

        //    decimal totalProfit = profitTransactions.Sum(x => x.Amount);

        //    decimal maxOutProfit = totalStaking * 3;

        //    return maxOutProfit <= totalProfit;
        //}

        public MaxProfitViewModel GetMaxProfitSummary(string appUserId)
        {
            MaxProfitViewModel response = new() { };

            var appUser = _userManager.FindByIdAsync(appUserId).Result;

            var transactions = _walletTransactionService.GetAllByUserId(appUser.Id);

            var profitTransactions = transactions
                 .Where(x => x.Type == WalletTransactionType.StakingLeadership
                 || x.Type == WalletTransactionType.StakingProfit
                 || x.Type == WalletTransactionType.StakingAffiliateOnProfit
                 || x.Type == WalletTransactionType.StakingReferralDirect);

            decimal totalProfit = profitTransactions.Sum(x => x.Amount);


            decimal totalStaking = 0;

            decimal maxOutProfit = totalStaking * 3;

            if (totalStaking == 0)
                response.IsMaxOut = true;
            else
                response.IsMaxOut = maxOutProfit <= totalProfit;

            response.RemainProfit = maxOutProfit - totalProfit;

            return response;
        }

        public async Task<int> ReferralStakingLevelSummary(string appUserId)
        {

            var referralF1 = await _userManager.FindByIdAsync(appUserId);

            var f1Count = _userManager.Users.Count(x => 0 >= 2000
                && !x.IsSystem
                && x.ReferralId == referralF1.Id);

            var referralLevel = 0;

            for (int i = 3; i < 11; i++)
            {
                if (f1Count >= i)
                    referralLevel++;
            }

            return referralLevel;

        }

        public async Task SendChannelTeleGroup(decimal investAmount,Unit unit, AppUser appUser)
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
            var message = TelegramBotHelper.BuildReportStakingMessage
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

        public async Task RandomStakingShowUser()
        {
            // Get list user show off
            //try
            //{
            //    StakingPackage stakingPakage = StakingPackage.Package1000;

            //    Random random = new();
            //    do
            //    {
            //        stakingPakage = random.NextEnumValue<StakingPackage>();

            //    } while ((int)stakingPakage > 10000 || (int)stakingPakage < 1000);

            //    var minimumBalance = (int)stakingPakage;

            //    var latestStakingOfShowUser = _stakingRepository.FindAll(x => x.AppUser)
            //        .Where(x => x.AppUser.IsShowOff)
            //        .OrderByDescending(x => x.Id).FirstOrDefault();

            //    Guid stakingUser = Guid.Empty;


            //    if (latestStakingOfShowUser == null)
            //    {
            //        stakingUser = _userManager.Users
            //        .Where(x => x.IsShowOff
            //        && x.BCAmount >= minimumBalance)
            //        .OrderBy(x => Guid.NewGuid()).Select(x => x.Id).FirstOrDefault();
            //    }
            //    else
            //    {
            //        stakingUser = _userManager.Users
            //        .Where(x => x.IsShowOff
            //        && x.Id != latestStakingOfShowUser.AppUserId
            //        && x.BCAmount >= minimumBalance)
            //        .OrderBy(x => Guid.NewGuid()).Select(x => x.Id).FirstOrDefault();
            //    }


            //    if (stakingUser == Guid.Empty)
            //        return;

            //    var stakingModel = new BuyStakingViewModel
            //    {
            //        Package = stakingPakage,
            //        Unit = Unit.ProjectToken
            //    };

            //    var modelJson = JsonConvert.SerializeObject(stakingModel);

            //    var result = await StakingToken(modelJson, stakingUser.ToString());

            //    if (result.Success) { }
            //}
            //catch (Exception e)
            //{
            //    throw;
            //}
        }

        public Task<GenericResult> StakingToken(string modelJson, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<GenericResult> ProcessPaymentAffiliateInsurance(Guid appUserId, Unit unit)
        {
            throw new NotImplementedException();
        }

        public async Task<GenericResult> CancelStaking(Guid appUserId,int stakingId)
        {
            try
            {
                var appUser = await _userManager.FindByIdAsync(appUserId.ToString());

                var staking = _stakingRepository.FindById(stakingId);

                if (appUser == null || staking.AppUserId != appUserId)
                    return new GenericResult(false, "Staking not found");

                int stakingDate = (DateTime.Now - staking.DateCreated).Days;

                if (stakingDate < 31)
                    return new GenericResult(false, "Min staking days is 31 to cancel staking");

                decimal refundRate = 0;

                if (stakingDate < 181)
                    refundRate = 0.5m;
                if (stakingDate >= 181 && stakingDate <= 290)
                    refundRate = 0.85m;
                if (stakingDate > 290)
                    refundRate = 1;

                var refundAmount = refundRate * staking.StakingAmountUSDT;

                appUser.USDTAmount += refundAmount;

                var result = await _userManager.UpdateAsync(appUser);
                if (!result.Succeeded)
                    return new GenericResult(false, "Cancel fail, retry later");

                staking.CancelOn = DateTime.Now;
                staking.Type = StakingType.Cancel;
                _stakingRepository.Update(staking);
                _unitOfWork.Commit();

                var txnHash = Guid.NewGuid().ToString("N");

                _walletTransactionService.AddTransaction(
                    appUser.Id,
                    refundAmount,
                    refundAmount,
                    WalletTransactionType.CancelStaking,
                    "SYSTEM",
                    $"Wallet {Unit.USDT.GetDescription()}",
                    Unit.USDT,
                    0,
                    0,
                    txnHash,
                    "Cancel staking from " + appUser.Sponsor + " and refund " + refundAmount + " " + Data.Enums.Unit.USDT.GetDescription() + $" for staking {stakingDate} days");
                return new GenericResult(true, "Cancel staking is successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("StakingController_CancelStaking: {0}", ex.Message);
                return new GenericResult(false, ex.Message);
            }
        }

        public async Task<bool> ProcessDailyStaking()
        {
            try
            {
                var queryStakings = _stakingRepository.FindAll(x =>
                        x.Type == StakingType.Process
                        && x.AppUser.IsShowOff == false
                        && x.ReceiveTimes < 360
                        && x.DateCreated.Date < DateTime.Now.Date
                        && (x.ReceiveLatest == null || x.ReceiveLatest.Value.Date < DateTime.Now.Date))
                    .ToList();

                var interestRate = 0.05m; // 15% month

                foreach (var staking in queryStakings)
                {
                    var stakingObj = _stakingRepository.FindById(staking.Id);

                    var appUser = await _userManager.FindByIdAsync(staking.AppUserId.ToString());

                    if (appUser == null)
                        continue;

                    var stakingProfit = interestRate * staking.StakingAmount;

                    var txnHash = Guid.NewGuid().ToString("N");

                    appUser.PINetworkAmount += stakingProfit;

                    var result = await _userManager.UpdateAsync(appUser);

                    if (!result.Succeeded)
                    {
                        //Please write log when has exception
                        _logger.LogError($"ProcessDailyStaking: AppUser {appUser.Email}" +
                            $" ,StakingId : {stakingObj.Id} " +
                            $" ,staking profit: {stakingProfit} ");

                        continue;
                    }

                    stakingObj.ReceiveLatest = DateTime.Now;
                    stakingObj.ReceiveTimes += 1;
                    stakingObj.DateUpdated = DateTime.Now;
                    _stakingRepository.Update(stakingObj);

                    _walletTransactionService.AddTransaction(appUser.Id,
                    stakingProfit,
                    stakingProfit,
                    WalletTransactionType.StakingProfit,
                    "SYSTEM",
                    $"Wallet Staking",
                    Unit.PI,
                    0,
                    0,
                    txnHash,
                    $"Payment invest profit to {appUser.Sponsor} by " +
                    $"{stakingProfit} {Unit.PI.GetDescription()} from Staking PI" +
                    $"{stakingObj.StakingAmount}");

                    var rewardId = SaveStakingReward(appUser,stakingObj,0.05m,stakingProfit);

                    await CompletedStakingCheck(stakingObj, appUser);

                    await PaymentCommissionOnProfit(stakingProfit, stakingObj.StakingAmountUSDT,appUser, rewardId);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ProcessDailyInvestProfit: Exception {ex.Message} {ex.StackTrace}");
            }

            return true;
        }

        async Task<bool> CompletedStakingCheck(Staking staking, AppUser appUser)
        {
            if (staking.ReceiveTimes < 360)
                return false;
            
            staking.CompletedOn = DateTime.Now;
            staking.DateUpdated = DateTime.Now;
            staking.Type = StakingType.Finish;
            staking.Remarks += $" Completed invest, refund 100% invest amount and bonus 100% invest amount";

            _stakingRepository.Update(staking);

            appUser.USDTAmount += staking.StakingAmountUSDT;

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                _logger.LogError($"CompletedStakingCheck: AppUser {appUser.Email}" +
                            $" ,Staking ID: {staking.Id} " +
                            $" ,USDTAmount: {staking.StakingAmountUSDT} ");

                //Please write log when has exception

                return false;
            }

            var txnHash = Guid.NewGuid().ToString("N");

            _walletTransactionService.AddTransaction(appUser.Id,
                staking.StakingAmountUSDT,
                staking.StakingAmountUSDT,
                WalletTransactionType.RefundCompletedStaking,
                "SYSTEM",
                $"Wallet {Unit.USDT.GetDescription()}",
                Unit.USDT,
                0,
                0,
                txnHash,
                $"Refund invest amount to {appUser.Sponsor} by " +
                $"{staking.StakingAmountUSDT} {Unit.USDT.GetDescription()} from Completed Staking " +
                $"{staking.StakingAmount} " +
                $"{Unit.PI.GetDescription()}");

            
            return true;
        }

        async Task<bool> PaymentCommissionOnProfit(
            decimal profitAmount, decimal investAmountInUSDT, AppUser appUser, int rewardId)
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

                f1.StakingPIAffiliateAmount += profitBonus;

                var ret = await _userManager.UpdateAsync(f1);
                if (ret.Succeeded)
                {
                    #region Save Referral Report Info

                    AddStakingAffiliate(f1, profitBonus, 30, rewardId);

                    #endregion

                    _walletTransactionService.AddTransaction(f1.Id,
                    profitBonus,
                    profitBonus,
                    WalletTransactionType.StakingAffiliateOnProfit,
                    "SYSTEM",
                    $"Wallet Staking Affiliate",
                    Unit.PI,
                    0,
                    0,
                    txnHash,
                    $"Invest profit referral F1 30% of {profitAmount} from {appUser.Sponsor} = " +
                    $"{profitBonus} {Unit.PI.GetDescription()} ");
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

                f2.StakingPIAffiliateAmount += profitBonus;

                var ret = await _userManager.UpdateAsync(f2);
                if (ret.Succeeded)
                {
                    #region Save Referral Report Info
                    AddStakingAffiliate(f2, profitBonus, 10, rewardId);
                    #endregion

                    _walletTransactionService.AddTransaction(f2.Id,
                    profitBonus,
                    profitBonus,
                    WalletTransactionType.StakingAffiliateOnProfit,
                    "SYSTEM",
                    $"Wallet Staking Affiliate",
                    Unit.PI,
                    0,
                    0,
                    txnHash,
                    $"Invest profit referral F2 10% of {profitAmount} from {appUser.Sponsor} = " +
                    $"{profitBonus} {Unit.PI.GetDescription()} ");
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

                f3.StakingPIAffiliateAmount += profitBonus;

                var ret = await _userManager.UpdateAsync(f3);
                if (ret.Succeeded)
                {
                    #region Save Referral Report Info
                    AddStakingAffiliate(f3, profitBonus, 10, rewardId);
                    #endregion

                    _walletTransactionService.AddTransaction(f2.Id,
                    profitBonus,
                    profitBonus,
                    WalletTransactionType.StakingAffiliateOnProfit,
                    "SYSTEM",
                    $"Wallet Staking Affiliate",
                    Unit.PI,
                    0,
                    0,
                    txnHash,
                    $"Invest profit referral f3 10% of {profitAmount} from {appUser.Sponsor} = " +
                    $"{profitBonus} {Unit.PI.GetDescription()} ");
                }
            }

            return true;
        }

        private void AddStakingAffiliate(AppUser appUser,
            decimal profit,
            decimal rate,
            int stakingRewardId)
        {
            _stakingAffiliateRepository.Add(new StakingAffiliate
            {
                AppUserId = appUser.Id,
                DateCreated = DateTime.Now,
                Amount = profit,
                StakingRewardId = stakingRewardId,
                Remarks = ""
            });

            _unitOfWork.Commit();
        }

        int SaveStakingReward(AppUser appUser, Staking staking, decimal interestRate, decimal profit)
        {
            var stakingReward = new StakingReward
            {
                Amount = profit,
                InterestRate = interestRate,
                AppUserId = appUser.Id,
                DateCreated = DateTime.Now,
                StakingId = staking.Id,
                ReferralId = appUser.Sponsor,
                Remarks = $"Profit {profit} {Unit.PI.GetDescription()} Rate: {interestRate} from staking {staking.StakingAmount} "
            };

            _stakingRewardRepository.Add(stakingReward);

            _unitOfWork.Commit();

            return stakingReward.Id;
        }
    }
}

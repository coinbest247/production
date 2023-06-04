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
    public class AffiliateAgentService : IAffiliateAgentService
    {
        private readonly IAgentCommissionRepository _agentCommissionRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ILogger<AffiliateAgentService> _logger;
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly IConfigService _configService;

        public AffiliateAgentService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IBlockChainService blockChainService,
            ILogger<AffiliateAgentService> logger,
            TelegramBotWrapper botTelegramService,
            IConfigService configService,
            IWalletTransactionService walletTransactionService,
            IAgentCommissionRepository agentCommissionRepository)
        {
            _configService = configService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _walletTransactionService = walletTransactionService;
            _botTelegramService = botTelegramService;
            _agentCommissionRepository = agentCommissionRepository;
        }


        public PagedResult<AgentCommissionViewModel> GetAgentCommissionPaging(
            string keyword, Guid appUserId, int pageIndex, int pageSize)
        {
            var query = _agentCommissionRepository.FindAll();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.FromSponsor.ToLower().Contains(keyword.ToLower()));

            query = query.Where(x => x.AppUserId == appUserId);

            var totalRow = query.Count();
            var data = query.OrderBy(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new AgentCommissionViewModel
                {
                    Id = x.Id,
                    ReferralLevel = x.ReferralLevel,
                    ProfitAmount = x.ProfitAmount,
                    DateCreated = x.DateCreated,
                    FromSponsor = x.FromSponsor,
                }).ToList();

            return new PagedResult<AgentCommissionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public async Task<GenericResult> ProcessAgentActivation(Guid appUserId, Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(appUserId.ToString());

            if (appUser.IsAffiliateAgent)
                return new GenericResult(false, "Affiliate agent was paid, cannot paymment again.");

            bool isValidPayment = false;

            decimal paymentAmount = 100;

            switch (unit)
            {
                case Unit.USDT:
                    if (appUser.USDTAmount >= 100)
                    {
                        appUser.USDTAmount -= 100;
                        isValidPayment = true;
                    }
                    break;
                case Unit.BUSD:
                    if (appUser.BUSDAmount >= 100)
                    {
                        appUser.BUSDAmount -= 100;
                        isValidPayment = true;
                    }
                    break;
                case Unit.BNB:
                    var bnbPrice = _blockChainService.GetCurrentPrice("BNB", "USD");
                    var bnbRequired = Math.Round(100 / bnbPrice, 4);
                    paymentAmount = bnbRequired;
                    if (bnbRequired > 0 && appUser.BNBAmount >= bnbRequired)
                    {
                        appUser.BNBAmount -= bnbRequired;
                        isValidPayment = true;
                    }
                    break;
                default:
                    break;
            }

            if (!isValidPayment)
                return new GenericResult(false, "Payment failed, retry later.");

            appUser.IsAffiliateAgent = true;

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
                return new GenericResult(false, "Payment failed, retry later.");

            var txnHash = Guid.NewGuid().ToString("N");

            _walletTransactionService.AddTransaction(appUser.Id,
                paymentAmount,
                paymentAmount,
                WalletTransactionType.AgentActivation,
                $"Wallet {unit.GetDescription()}",
                "SYSTEM",
                unit,
                0,
                0,
                txnHash,
                $"Payment agent activation from {appUser.Sponsor} by {paymentAmount} {unit.GetDescription()}");

            await ProcessAgentCommission(appUser);

            return new GenericResult(true, "Payment agent activation successfully.");
        }


        async Task<bool> ProcessAgentCommission(AppUser appUser)
        {
            if (appUser.ReferralId == null || appUser.IsShowOff)
                return false;

            var txnHash = Guid.NewGuid().ToString("N");

            var f1 = await _userManager.FindByIdAsync(appUser.ReferralId.Value.ToString());
            if (f1 == null || f1.IsShowOff || f1.IsSystem)
                return false;

            #region F1 Referral

            if (f1.IsAffiliateAgent && f1.InvestBotAmount > 0)
            {
                await ProcessAgentCommissionTransaction(f1,
                    30,
                    txnHash,
                    $"Payment F1 agent activation referral from " +
                    $"{appUser.Sponsor} by {30} " +
                    $"{Unit.USDT.GetDescription()}");

                AddAgentCommission(new AgentCommission
                {
                    AppUserId = f1.Id,
                    DateCreated = DateTime.Now,
                    FromAppUserId = appUser.Id,
                    FromSponsor = appUser.Sponsor,
                    ProfitAmount = 30,
                    ReferralLevel = 1
                });
            }
            #endregion

            #region F2 Referral

            if (f1.ReferralId == null)
                return false;

            var f2 = await _userManager.FindByIdAsync(f1.ReferralId.Value.ToString());

            if (f2 == null || f2.IsShowOff || f2.IsSystem) 
                return false;

            if (f2.IsAffiliateAgent && f2.InvestBotAmount > 0)
            {
                await ProcessAgentCommissionTransaction(f2,
                    10,
                    txnHash,
                    $"Payment F2 agent activation referral from " +
                    $"{appUser.Sponsor} by {10} " +
                    $"{Unit.USDT.GetDescription()}");

                AddAgentCommission(new AgentCommission
                {
                    AppUserId = f2.Id,
                    DateCreated = DateTime.Now,
                    FromAppUserId = appUser.Id,
                    FromSponsor = appUser.Sponsor,
                    ProfitAmount = 10,
                    ReferralLevel = 2
                });
            }

            #endregion

            #region F3 Referral

            if (f2.ReferralId == null)
                return false;

            var f3 = await _userManager.FindByIdAsync(f2.ReferralId.Value.ToString());

            if (f3 == null || f3.IsShowOff || f3.IsSystem) 
                return false;

            if (f3.IsAffiliateAgent && f3.InvestBotAmount > 0)
            {
                await ProcessAgentCommissionTransaction(f3,
                    10,
                    txnHash,
                    $"Payment F3 agent activation referral from " +
                    $"{appUser.Sponsor} by {10} " +
                    $"{Unit.USDT.GetDescription()}");

                AddAgentCommission(new AgentCommission
                {
                    AppUserId = f3.Id,
                    DateCreated = DateTime.Now,
                    FromAppUserId = appUser.Id,
                    FromSponsor = appUser.Sponsor,
                    ProfitAmount = 10,
                    ReferralLevel = 3
                });
            }

            #endregion

            return true;
        }

        async Task<bool> ProcessAgentCommissionTransaction(
            AppUser appUser, decimal amount, string txnHash, string remarks)
        {
            appUser.USDTAmount += amount;

            var result = await _userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                _walletTransactionService.AddTransaction(appUser.Id,
                amount,
                amount,
                WalletTransactionType.AgentCommission,
                "SYSTEM",
                $"Wallet {Unit.USDT.GetDescription()}",
                Unit.USDT,
                0,
                0,
                txnHash,
                remarks);

                return true;
            }

            return false;
        }

        private void AddAgentCommission(AgentCommission agentCommission)
        {
            _agentCommissionRepository.Add(agentCommission);

            _unitOfWork.Commit();
        }
    }
}

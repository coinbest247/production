using Core.Application.ViewModels.Staking;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IStakingService
    {
        PagedResult<StakingViewModel> GetAllPaging(string keyword,
            string appUserId,
            DateTime? fromDate,
            DateTime? toDate,
            int pageIndex, 
            int pageSize);

        List<StakingViewModel> GetLeaderboard(int top);

        IQueryable<Staking> GetAllByUserId(Guid appUserId);

        decimal GetTotalPackage(Guid userId, StakingType? type);

        Task<GenericResult> StakingToken(string modelJson, string userId);

        StakingViewModel GetById(int id);

        void Update(StakingViewModel model);

        void UpdateTxtHash(int id, string txtHash);

        Staking Add(StakingViewModel Model);

        decimal GetTotalStaking();

        decimal GetTodayStaking();


        void Save();

        Task<int> ReferralStakingLevelSummary(string appUserId);

        MaxProfitViewModel GetMaxProfitSummary(string appUserId);

        decimal GetMaxout(string appUserId);

        Task RandomStakingShowUser();

        Task<GenericResult> ProcessPaymentAffiliateInsurance(Guid appUserId, Unit unit);

        Task<GenericResult> PaymentStakingAsync(BuyStakingViewModel model, Guid userId);

        Task<GenericResult> CancelStaking(Guid appUserId, int stakingId);

        PagedResult<StakingRewardViewModel> GetAllProfitPaging(string keyword, string appUserId,
            DateTime? fromDate,
            DateTime? toDate,
            int pageIndex,
            int pageSize);

        Task<bool> ProcessDailyStaking();
    }
}

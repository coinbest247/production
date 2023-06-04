using Core.Application.ViewModels.AffiliateAgent;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.InvestPacakage;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IInvestPackageService
    {
        PagedResult<InvestPackageViewModel> GetAllPaging(string keyword, Guid appUserId, int type, int pageIndex, int pageSize);
        
        InvestPackageViewModel GetById(int id);

        void Insert(InvestPackageViewModel investPackageVm);

        Task<GenericResult> ProcessBuyInvestPackage(PaymentInvestModel model, string userId);

        Task<GenericResult> ProcessCancelInvestPackage(PaymentInvestModel model, string userId);

        Task<bool> ProcessDailyInvestProfit();

        PagedResult<InvestPackageAffiliateViewModel> GetAllInvestPackageAffiliatePaging(string keyword,
            Guid appUserId,
            int pageIndex,
            int pageSize);

        decimal GetTotalInvestBotTrade(List<Guid> appUserIds);

    }
}

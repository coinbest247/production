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
    public interface IAffiliateAgentService
    {
        Task<GenericResult> ProcessAgentActivation(Guid appUserId, Unit unit);

        PagedResult<AgentCommissionViewModel> GetAgentCommissionPaging(string keyword,
            Guid appUserId,
            int pageIndex,
            int pageSize);
    }
}

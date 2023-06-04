using Core.Application.ViewModels.InvestPacakage;
using Core.Utilities.Dtos;

namespace Core.Application.Interfaces
{
    public interface IInvestPackageRewardService
    {
        PagedResult<InvestPackageRewardViewModel> GetAllPaging(string keyword, string appUserId, int page, int pageSize);

        void Save();
    }
}

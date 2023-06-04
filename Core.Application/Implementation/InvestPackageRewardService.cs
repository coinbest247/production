using Core.Application.Interfaces;
using Core.Application.ViewModels.InvestPacakage;
using Core.Data.IRepositories;
using Core.Utilities.Dtos;
using System;
using System.Linq;

namespace Core.Application.Implementation
{
    public class InvestPackageRewardService : IInvestPackageRewardService
    {

        private readonly IInvestPackageRewardRepository _investPackageRewardRepository;

        public InvestPackageRewardService(
            IInvestPackageRewardRepository investPackageRewardRepository)
        {
            _investPackageRewardRepository = investPackageRewardRepository;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public PagedResult<InvestPackageRewardViewModel> GetAllPaging(string keyword,string appUserId, int page, int pageSize)
        {
            var query = _investPackageRewardRepository.FindAll();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.AppUser.Email.Contains(keyword));
            }
            if (!string.IsNullOrEmpty(appUserId))
                query = query.Where(x => x.AppUserId == Guid.Parse(appUserId));

            int totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize)
                .Take(pageSize).Select(x => new InvestPackageRewardViewModel()
                {
                    Id = x.Id,
                    Sponsor = x.Sponsor,
                    DateCreated = x.DateCreated,
                    InvestAmount = x.InvestAmount,
                    InterestRate = x.InterestRate,
                    Amount = x.Amount
                }).ToList();

            var paginationSet = new PagedResult<InvestPackageRewardViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };

            return paginationSet;
        }
    }
}

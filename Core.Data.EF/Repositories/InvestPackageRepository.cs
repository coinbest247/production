using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{
    public class InvestPackageRepository : EFRepository<InvestPackage, int>, IInvestPackageRepository
    {
        public InvestPackageRepository(AppDbContext context) : base(context)
        {
        }
    }
}

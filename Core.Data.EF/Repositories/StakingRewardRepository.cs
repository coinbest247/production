using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{

    public class StakingRewardRepository : EFRepository<StakingReward, int>, IStakingRewardRepository
    {
        AppDbContext _context;
        public StakingRewardRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}

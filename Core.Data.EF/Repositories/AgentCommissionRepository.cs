using Core.Data.Entities;
using Core.Data.IRepositories;

namespace Core.Data.EF.Repositories
{
    public class AgentCommissionRepository : EFRepository<AgentCommission, int>, IAgentCommissionRepository
    {
        public AgentCommissionRepository(AppDbContext context) : base(context)
        {
        }
    }
}

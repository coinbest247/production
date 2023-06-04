using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.AffiliateAgent
{
    public class AgentCommissionViewModel
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }

        public Guid AppUserId { get; set; }

        public Guid FromAppUserId { get; set; }

        public string FromSponsor { get; set; }

        public int ReferralLevel { get; set; }

        public decimal ProfitAmount { get; set; }
    }
}

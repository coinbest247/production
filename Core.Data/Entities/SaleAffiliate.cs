using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entities
{
    [Table("SaleAffiliates")]
    public class SaleAffiliate : DomainEntity<int>
    {
        public DateTime DateCreated { get; set; }

        public Guid AppUserId { get; set; }

        public Guid FromAppUserId { get; set; }

        public string FromSponsor { get; set; }

        public int ReferralLevel { get; set; }

        public decimal ProfitAmount { get; set; }

        public decimal InterestRate { get; set; }

        public decimal USDAmount { get; set; }

        public decimal Amount { get; set; } 

        public Unit Unit { get; set; }
    }
}

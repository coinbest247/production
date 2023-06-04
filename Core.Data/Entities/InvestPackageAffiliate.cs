using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("InvestPackageAffiliates")]
    public class InvestPackageAffiliate : DomainEntity<int>
    {
        public DateTime DateCreated { get; set; }

        public Guid AppUserId { get; set; }

        public Guid FromAppUserId { get; set; }

        public string FromSponsor { get; set; }

        public int ReferralLevel { get; set; }

        public decimal ProfitAmount { get; set; }

        public decimal InterestRate { get; set; }

        public decimal InvestAmountInUSDT { get; set; }
    }
}

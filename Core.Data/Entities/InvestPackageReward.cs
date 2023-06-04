using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("InvestPackageRewards")]
    public class InvestPackageReward : DomainEntity<int>
    {
        [Required]
        public decimal InterestRate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public int InvestId { get; set; }

        [Required]
        public Guid AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        public string Sponsor { get;set;}

        public decimal InvestAmount { get;set; }    

        //public virtual ICollection<StakingAffiliate> StakingAffiliates { get; set; }
    }
}

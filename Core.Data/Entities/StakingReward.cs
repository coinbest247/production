using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entities
{
    [Table("StakingRewards")]
    public class StakingReward : DomainEntity<int>
    {
        [Required]
        public decimal InterestRate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public int StakingId { get; set; }

        [Required]
        public Guid AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        [ForeignKey("StakingId")]
        public virtual Staking Staking { set; get; }

        public string ReferralId { get;set; }   

        public string Remarks { get;set; } 
    }
}

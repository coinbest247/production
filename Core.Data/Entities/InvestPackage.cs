using Core.Infrastructure.SharedKernel;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Core.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.Data.Entities
{
    [Table("InvestPackages")]
    public class InvestPackage : DomainEntity<int>
    {
        public decimal InvestAmount { get; set; }

        public Unit Unit { get; set; }

        public decimal USDAmount { get; set; }

        public InvestPackageType Type { get; set; } 

        public int ReceivedCount { get;set; }

        public DateTime? LastReceived { get; set; }

        public DateTime? CancelOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public string Remarks { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        [Required]
        public Guid AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }
    }
}

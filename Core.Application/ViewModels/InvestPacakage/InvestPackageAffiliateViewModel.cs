using System;

namespace Core.Application.ViewModels.InvestPacakage
{
    public class InvestPackageAffiliateViewModel
    {
        public int Id { get; set; } 
        public DateTime DateCreated { get; set; }

        public Guid AppUserId { get; set; }

        public Guid FromAppUserId { get; set; }

        public string FromSponsor { get; set; } 

        public int ReferralLevel { get; set; }  

        public decimal ProfitAmount { get;set; }

        public decimal InterestRate { get;set; }

        public decimal InvestAmountInUSDT { get;set; }
    }
}

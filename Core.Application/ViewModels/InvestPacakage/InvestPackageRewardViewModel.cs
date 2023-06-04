using Core.Data.Entities;
using System;

namespace Core.Application.ViewModels.InvestPacakage
{
    public class InvestPackageRewardViewModel
    {
        public int Id { get;set; }  

        public decimal InvestAmount { get;set; }

        public decimal InterestRate { get; set; }

        public decimal Amount { get; set; }

        public DateTime DateCreated { get; set; }

        public int InvestId { get; set; }


        public Guid AppUserId { get; set; }


        public string Sponsor { get;set; }

    }
}

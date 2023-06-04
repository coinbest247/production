using Core.Data.Enums;
using System;

namespace Core.Application.ViewModels.InvestPacakage
{
    public class InvestPackageViewModel
    {
        public int Id { get; set; }

        public string Remarks { get; set; }

        public Guid AppUserId { get; set; }

        public decimal InvestAmount { get; set; }

        public InvestPackageType Type { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public int ReceivedCount { get; set; }

        public DateTime? LastReceived { get; set; }

        public DateTime? CancelOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public string TypeName { get;set; }

        public Unit Unit { get; set; }

        public decimal USDAmount { get; set; }

        public string UnitName { get; set; }
    }
}

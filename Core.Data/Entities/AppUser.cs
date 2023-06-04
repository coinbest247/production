using BeCoreApp.Data.Entities;
using BeCoreApp.Data.Enums;
using Core.Data.Enums;
using Core.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("AppUsers")]
    public class AppUser : IdentityUser<Guid>, IDateTracking, ISwitchable
    {
        public string Sponsor { get; set; }

        public Guid? ReferralId { get; set; }

        public string PublishKey { get; set; }

        public string PrivateKey { get; set; }

        public decimal USDTAmount { get; set; }

        public decimal BNBAmount { get; set; }

        public decimal ShibaAmount { get; set; }

        public decimal BUSDAmount { get; set; }

        public decimal BCAmount { get; set; }

        public decimal InvestBotAmount { get; set; }

        public bool IsShowOff { get; set; }

        public bool IsRejectWithdraw { get; set; }

        public bool IsRejectInterest { get; set; }

        public bool IsRejectSwap { get; set; }

        public bool IsRejectTransfer { get; set; }

        public DateTime? LastClaimReward { get; set; }

        public bool IsAffiliateAgent { get; set; }

        public decimal BotTradeAmount { get; set; }

        public decimal BCClaimAmount { get; set; }

        public decimal SHIBClaimAmount { get; set; }

        public decimal BCFutureAmount { get; set; }

        public decimal USDTFutureAmount { get; set; }

        public decimal BNBFutureAmount { get; set; }

        public decimal StakingPIAmount { get; set; }

        public decimal PINetworkAmount { get; set; }

        public decimal PiNetworkFutureAmount { get; set; }

        

        public decimal StakingPIAffiliateAmount { get; set; }

        public bool IsSystem { get; set; } = false;

        public Status Status { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public string ByCreated { get; set; }

        public string ByModified { get; set; }

        public virtual ICollection<Support> Supports { set; get; }
        public virtual ICollection<TicketTransaction> TicketTransactions { set; get; }
        public virtual ICollection<WalletTransaction> WalletTransactions { set; get; }
        public virtual ICollection<Airdrop> Airdrops { set; get; }
        public virtual ICollection<InvestPackage> InvestPackages { set; get; }

    }
}

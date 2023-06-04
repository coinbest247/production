using Core.Utilities.Constants;
using System.ComponentModel;

namespace Core.Data.Enums
{
    public enum WalletTransactionType
    {
        [Description("Deposit")]
        Deposit = 1,

        [Description("Withdraw")]
        Withdraw = 2,

        [Description("Transfer")]
        Transfer = 3,

        [Description("Received")]
        Received = 4,

        [Description("Staking")]
        Staking = 5,
        [Description("Staking Profit")]
        StakingProfit = 6,
        [Description("Staking Referral Direct")]
        StakingReferralDirect = 7,
        [Description("Staking Affiliate on Profit")]
        StakingAffiliateOnProfit = 8,
        [Description("Staking Leadership")]
        StakingLeadership = 9,
        
        [Description("Swap From "+ CommonConstants.TOKEN_CODE)]
        SwapFromProjectToken = 10,
        [Description("Swap To USDT")]
        SwapToUSDT = 11,

        [Description("Airdrop")]
        Airdrop = 12,

        [Description("Verification Account Bonus")]
        VerificationAccountBonus = 13,
        [Description("Claim")]
        Claim = 14,
        [Description("Agent Activation")]
        AgentActivation = 15,
        [Description("Agent Commission")]
        AgentCommission = 16,
        [Description("Invest Bot")]
        InvestBot = 17,
        [Description("Cancel Bot")]
        CancelBot = 18,
        [Description("Profit Bot")]
        ProfitBot = 19,
        [Description("Completed Bot")]
        CompletedBot = 20,
        [Description("Bonus Completed Bot")]
        BonusCompletedBot = 21,
        [Description("Commission on Profit Bot")]
        CommissionOnProfitBot = 22,
        [Description("Buy Token")]
        BuyToken = 23,
        [Description("Transfer To Future")]
        TransferFuture = 24,
        [Description("Transfer To Main")]
        TransferMain = 25,
        [Description("Sale Affiliate")]
        SaleAffiliate = 26,
        [Description("Cancel Staking")]
        CancelStaking = 27,
        [Description("Bonus Completed Staking")]
        BonusCompletedStaking = 28,
        [Description("Refund Completed Staking")]
        RefundCompletedStaking = 29
    }
}

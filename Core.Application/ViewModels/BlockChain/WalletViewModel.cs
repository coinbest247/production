using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.BlockChain
{
    public class WalletViewModel
    {
        public string AuthenticatorCode { get; set; }
        public bool Enabled2FA { get; set; }
        public string PrivateKey { get; set; }
        public string PublishKey { get; set; }

        public decimal USDTAmount { get; set; }
        public decimal BCAmount { get; set; }

        public decimal BNBAmount { get; set; }

        public decimal BUSDAmount { get; set; } 

        public decimal SHIBAmount { get; set; }

        public decimal SHIBClaimAmount { get; set; }

        public decimal BCClaimAmount { get;set; }

        public decimal BotTradeAmount { get;set; }

        public decimal BCFutureAmount { get; set; }

        public decimal USDTFutureAmount { get; set; }

        public decimal PiNetworkAmount { get; set; }

        public decimal PiNetworkFutureAmount { get; set; }
    }
}

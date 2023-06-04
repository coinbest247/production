using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.Exchange
{
    public class ExchangeBalanceViewModel
    {
        public decimal Balance { get; set; }

        public decimal CoinPrice { get;set; }

        public decimal TokenPrice { get;set; }

        public decimal MinPayment { get; set; }
    }
}

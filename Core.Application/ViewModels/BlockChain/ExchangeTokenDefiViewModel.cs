using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.BlockChain
{
    public class ExchangeTokenDefiViewModel
    {
        public decimal OrderBNB { get; set; }
        public decimal AmountToken { get; set; }

        public decimal TokenPrice { get; set; }
        public string TokenPriceString { get; set; }
        public decimal BNBPrice { get; set; }
    }
}

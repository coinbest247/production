using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum StakingPackage
    {
        // < $3,000 profit 8%
        [Description("Basic")]
        Package1000 = 1000,

        [Description("Startup")]
        Package2000 = 2000,

        [Description("Standard")]
        Package5000 = 5000,

        [Description("Advanced")]
        Package10000 = 10000,

        [Description("Member")]
        Package20000 = 20000,


        // 3,000$ < 10,000$ profit 10%
        [Description("Enterprise")]
        Package30000 = 30000,

        [Description("Gold")]
        Package50000 = 50000,

        // >= 10,000$ profit 12%
        [Description("Vip")]
        Package100000 = 100000,

        [Description("Silver")]
        Package200000 = 200000,

        [Description("Sapphire")]
        Package500000 = 500000,

        [Description("Ruby")]
        Package1000000 = 1000000,

        [Description("Diamond")]
        Package2000000 = 2000000,
    }
}
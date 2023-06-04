using Core.Utilities.Constants;
using System.ComponentModel;

namespace Core.Data.Enums
{
    public enum Unit
    {
        [Description(CommonConstants.TOKEN_CODE)]
        Token = 1,
        [Description("USDT")]
        USDT = 2,
        [Description("BUSD")]
        BUSD = 3,
        [Description("SHIB")]
        SHIBAINU = 4,
        [Description("BNB")]
        BNB = 5,
        [Description("PI")]
        PI = 6
    }
}

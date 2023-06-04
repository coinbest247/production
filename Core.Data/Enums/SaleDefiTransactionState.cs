using System.ComponentModel;

namespace Core.Data.Enums
{
    public enum SaleDefiTransactionState
    {
        [Description("None")]
        None = 0,
        [Description("Requested")]
        Requested = 1,
        [Description("Confirmed")]
        Confirmed = 2,
        [Description("Rejected")]
        Rejected = 3,
        [Description("Failed")]
        Failed = 4,
    }


}

using System.ComponentModel;

namespace Core.Data.Enums
{
    public enum InvestPackageType
    {
        [Description("Active")]
        Active = 1,
        [Description("Cancel")]
        Cancel = 2,
        [Description("Completed")]
        Completed = 3
    }
}

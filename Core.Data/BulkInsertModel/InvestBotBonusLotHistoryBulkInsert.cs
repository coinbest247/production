using System;
using System.Collections.Generic;
using System.Text;

namespace BeCoreApp.Data.BulkInsertModel
{
    public class InvestBotBonusLotHistoryBulkInsert
    {
        public Guid AppUserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public decimal BonusAmount { get; set; }

        public Guid RefereeAppUserId { get; set; }

        public decimal BonusRate { get; set; }

        public decimal RefereeInvestAmount { get; set; }

        public int RefereeRank { get; set; }
    }
}

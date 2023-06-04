using System;
using System.Collections.Generic;
using System.Text;

namespace BeCoreApp.Data.BulkInsertModel
{
    public class InvestBotProfitHistoryBulkInsert
    {
        public Guid AppUserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public int InvestBotHistoryId { get; set; }

        public decimal Profit { get; set; }

        public decimal ProfitPercentage { get; set; }

        public decimal InvestAmount { get; set; }
    }
}

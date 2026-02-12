using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class yearBudgetComparisonResponse
    {
        public int baseYear { get; set; }
        public decimal? baseTotalAllocated { get; set; }
        public decimal? baseTotalApproved{ get; set; }
        public decimal? baseTotalRequested { get; set; }

        //COMPARED YEAR
        public int comparedYear { get; set; }
        public decimal? comparedBaseTotalAllocated { get; set; }
        public decimal? comparedBaseTotalApproved { get; set; }
        public decimal? comparedBaseTotalRequested { get; set; }
    }
}

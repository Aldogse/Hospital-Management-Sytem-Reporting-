using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.BillingResponse
{
    public class yearsBillingReportComparisons
    {
        //BASE YEAR
        public int baseYear { get; set; }
        public decimal? total_billed { get; set; }
        public decimal? total_paid { get; set; }
        public int? total_pending_transaction { get; set; }
        public decimal? total_oop_collected { get; set; }
        public decimal? total_insurance_covered { get; set; }
        public decimal? total_pending_amount { get; set; }

        //COMPARED YEAR
        public int comparedYear { get; set; }
        public decimal? prev_total_billed { get; set; }
        public decimal? prev_total_paid { get; set; }
        public int? prev_total_pending_transaction { get; set; }
        public decimal? prev_total_oop_collected { get; set; }
        public decimal? prev_total_insurance_covered { get; set; }
        public decimal? prev_total_pending_amount { get; set; }

        //COMPARISON RESULTS
    }
}

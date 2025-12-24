using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class yearlyBillingReportResponse
    {
        public int year { get; set; }
        public decimal? total_billed { get; set; }
        public decimal? total_paid { get; set; }
        public int? total_pending_transactions { get; set; }
        public decimal? total_pending_amount { get; set; }
        public decimal? total_oop_collected { get; set; }
        public decimal? total_insurance_covered { get; set; }
        public List<monthBillingReportResponse> monthReports { get; set; }
    }
}

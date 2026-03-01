using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.claim_response
{
    public class dateRangeSummaryClaimQueryResponse
    {
        public decimal? claim_amount_submitted { get; set; }
        public decimal claims_amount_denied { get; set; }
        public int number_of_claims_submitted { get; set; }
        public int claims_approved { get; set; }
        public int claims_denied { get; set; }
        public int claims_pending { get; set; }
        public List<daily_insurance_submitted_report>days { get; set; }
    }
}

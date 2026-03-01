using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;
using Report_and_Analytics_Library.Insurance;

namespace APIResponses.claim_response
{
    public class monthInsuranceClaimRangeQuery
    {
        public int total_claims { get; set; }
        public int total_approved_claims { get; set; }
        public int total_denied_claims { get; set; }
        public decimal? total_amount_denied { get; set; }
        public decimal? total_amount_approved { get; set; }
        public List<monthly_claim_report>months { get; set; }
        public List<provider_claim_report>providers { get; set; }
    }
}

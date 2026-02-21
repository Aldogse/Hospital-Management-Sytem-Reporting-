using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.claim_response
{
    public class yearlyClaimReportResponse
    {
        public int year { get; set; }
        public int total_claims { get; set; }
        public int total_approved_claims { get; set; }
        public int total_denied_claims { get; set; }
        public List<monthly_claim_report> monthsClaim { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.claim_response
{
    public class monthsComparisonClaimResponse
    {
        public int basemonth { get; set; }
        public int baseyear { get; set; }
        public int base_total_claims { get; set; }
        public int base_total_approved_claims { get; set; }
        public int base_total_denied_claims { get; set; }

        public int partnermonth { get; set; }
        public int partneryear { get; set; }
        public int partner_total_claims { get; set; }
        public int partner_total_approved_claims { get; set; }
        public int partner_total_denied_claims { get; set; }
    }
}

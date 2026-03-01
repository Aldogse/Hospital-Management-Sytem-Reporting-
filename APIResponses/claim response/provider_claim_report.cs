using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.claim_response
{
    public class provider_claim_report
    {
        public int provider_id { get; set; }
        public string provider_name { get; set; }

        public int total_claims { get; set; }
        public int approved_claims { get; set; }
        public int denied_claims { get; set; }

        public decimal? approved_amount { get; set; }
        public decimal? denied_amount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.claim_response
{
    public class monthProviderClaimReport
    {
        public int month { get; set; }
        public int year { get; set; }
        public int provider_id { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal declinedAmount { get; set; }
    }
}

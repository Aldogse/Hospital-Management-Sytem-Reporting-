using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Insurance;

namespace APIResponses.claim_response
{
    public class monthClaimsHistory
    {
        public int month{ get; set; }
        public int year { get; set; }
        public int? totalClaims { get; set; }
        public int? totalApprovedClaims { get; set; }
        public int? totalDeniedClaims { get; set; }
        public List<insurance_claims> claimsList{ get; set; }

    }
}

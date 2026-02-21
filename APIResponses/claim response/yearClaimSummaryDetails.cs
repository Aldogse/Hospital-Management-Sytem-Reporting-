using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.claim_response
{
    public class yearClaimSummaryDetails
    {
        public int year { get; set; }
        public decimal? totalApprovePayoutAmount { get; set; }
        public int totalClaimApproved { get; set; }
        public int totalClaimDenied { get; set; }
        public decimal? totalHospitalLoss { get; set; }
    }
}

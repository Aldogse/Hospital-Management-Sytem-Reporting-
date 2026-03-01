using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class yearly_claim_report
    {
        [Key]
        public int report_id { get; set; }
        public int year { get; set; }
        public int total_claims { get; set; }
        public int total_approved_claims { get; set; }
        public int total_denied_claims { get; set; }
        public int lastInsuranceClaimIdProcessed { get; set; }
        public decimal? total_amount_paid { get; set; }
        public decimal? total_amount_denied { get; set; }
    }
}

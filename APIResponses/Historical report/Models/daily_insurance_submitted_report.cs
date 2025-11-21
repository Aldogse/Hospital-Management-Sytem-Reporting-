using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class daily_insurance_submitted_report
    {
        [Key]
        public int report_id { get; set; }
        public DateTime report_date { get; set; }
        public decimal? claim_amount { get; set; }
        public int number_of_claims_submitted { get; set; }
        public int claims_approved { get; set; }
    }
}
                                                                                                                    
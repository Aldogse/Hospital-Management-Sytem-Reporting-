using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class monthly_claim_report
    {
        [Key]
        public int reportId { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int pendingClaims { get; set; }
        public int approveClaims { get; set; }
        public int declinedClaims { get; set; }
    }
}

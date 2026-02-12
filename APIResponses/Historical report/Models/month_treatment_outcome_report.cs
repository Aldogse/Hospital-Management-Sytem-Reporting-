using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_treatment_outcome_report
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int total_transactions { get; set; }
        public int total_paid_count { get; set; }
        public int total_pending_count { get; set; }
        public int total_cancelled_count { get; set; }
        public decimal? total_paid_services { get; set; }
        public decimal? total_pending_amount_services { get; set; }
    }
}

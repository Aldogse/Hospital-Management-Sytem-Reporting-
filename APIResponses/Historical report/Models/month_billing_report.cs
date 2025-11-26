using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_billing_report
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public decimal? total_billed  { get; set; }
        public decimal? total_paid { get; set; }
        public decimal? total_pending { get; set; }
        public decimal? total_oop_collected { get; set; }
        public decimal? total_insurance_covered { get; set; }

    }
}

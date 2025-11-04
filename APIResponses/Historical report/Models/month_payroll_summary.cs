using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_payroll_summary
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int total_employees { get; set; }
        public decimal? total_gross_pay { get; set; }
        public decimal? total_deductions { get; set; }
        public decimal? total_net_pay { get; set; }
    }
}

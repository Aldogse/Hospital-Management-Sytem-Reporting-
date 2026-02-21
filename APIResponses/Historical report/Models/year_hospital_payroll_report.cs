using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class year_hospital_payroll_report
    {
        [Key]
        public int reportId { get; set; }
        public int year { get; set; }

        //only count the month of december
        public int total_employees { get; set; }
        public decimal? year_total_gross_pay { get; set; }
        public decimal? year_total_deductions { get; set; }
        public decimal? year_total_net_pay { get; set; }
    }
}

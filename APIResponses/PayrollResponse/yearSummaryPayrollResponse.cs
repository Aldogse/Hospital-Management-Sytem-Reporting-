using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.PayrollResponse
{
    public class yearSummaryPayrollResponse
    {
        public int? year { get; set; }

        //only count the month of december
        public int? total_employees { get; set; }
        public decimal? year_total_gross_pay { get; set; }
        public decimal? year_total_deductions { get; set; }
        public decimal? year_total_net_pay { get; set; }
        public List<month_payroll_summary>? monthsPayroll { get; set; }
    }
}

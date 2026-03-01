using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.PayrollResponse
{
    public class monthPayrollQueryRangeResponse
    {
        public int total_employees { get; set; }
        public decimal? total_gross_pay { get; set; }
        public decimal? total_deductions { get; set; }
        public decimal? total_net_pay { get; set; }
        public List<month_payroll_summary>months { get; set; }
    }
}

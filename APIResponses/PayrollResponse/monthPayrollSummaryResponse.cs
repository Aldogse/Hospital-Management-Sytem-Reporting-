using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.PayrollResponse
{
    public class monthPayrollSummaryResponse
    {
        public int? totalEmployees { get; set; }
        public decimal? totalNetPay { get; set; }
        public decimal? totalGrossPay { get; set; }
        public decimal? totalDeductions { get; set; }
        public List<month_payroll_summary> monthsRecords { get; set; }
    }
}

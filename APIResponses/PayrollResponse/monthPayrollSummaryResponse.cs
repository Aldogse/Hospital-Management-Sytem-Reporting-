using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PayrollResponse
{
    public class monthPayrollSummaryResponse
    {
        public int totalEmployees { get; set; }
        public decimal? totalNetPay { get; set; }
        public decimal? totalGrossPay { get; set; }
        public decimal? totalDeductions { get; set; }
        public List<individualPayrollSummaryReport> summaryList { get; set; }
    }
}

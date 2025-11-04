using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PayrollResponse
{
    public class yearSummaryPayrollResponse
    {
        public int month { get; set; }
        public decimal? totalDeductions { get; set; }
        public decimal? totalNetPay { get; set; }
        public int totalEmployees { get; set; }
    }
}

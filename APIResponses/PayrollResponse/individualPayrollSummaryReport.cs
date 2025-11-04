using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PayrollResponse
{
    public class individualPayrollSummaryReport
    {
        public string employeeName { get; set; }
        public decimal? grossPay { get; set; }
        public decimal? netPay { get; set; }
        public decimal? totalDeductions { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses
{
    public class payrollStatementResponses
    {
        public string employeeName { get; set; }
        public string? payPeriodStartDate { get; set; }
        public string? payPeriodEndDate { get; set; }
        public decimal? basicPay { get; set; }
        public decimal overtimePay { get; set; }
        public decimal? grossPay { get; set; }
        public decimal? totalDeductions { get; set; }
        public decimal? netPay { get; set; }
        public decimal sssDeduction { get; set; }
        public decimal philHealthDeduction { get; set; }
        public decimal? loanDeduction { get; set; }
        public decimal? absenceDeduction { get; set; }
        public string? dateGenerated { get; set; }
    }
}

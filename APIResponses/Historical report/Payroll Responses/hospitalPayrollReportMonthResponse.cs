using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Payroll_Responses
{
    public class hospitalPayrollReportMonthResponse
    {
        public int employeeId { get; set; }
        public string fullName { get; set; }
        public string department { get; set; }
        public string role { get; set; }
        public decimal? basicSalary { get; set; }
        public decimal? overtimePay { get; set; }
        public decimal? deductions { get; set; }
        public decimal? netPay { get; set; }
        public decimal? totalSalaryPaid { get; set; }
    }
}

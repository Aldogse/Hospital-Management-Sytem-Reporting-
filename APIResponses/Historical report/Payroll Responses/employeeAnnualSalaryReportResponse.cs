using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses
{
    public class employeeAnnualSalaryReportResponse
    {

        public string employeeName { get; set; }
        public decimal? yearTotalHoursWorked { get; set; }
        public decimal? yearTotalOvertimeHoursWorked { get; set; }
        public decimal? yearTotalWage { get; set; }
    }
}
                 
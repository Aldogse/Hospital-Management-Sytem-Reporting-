using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report
{
    public class employeeMonthsBreakdownResponse
    {
        public decimal? monthOvertimeHours { get; set; }
        public decimal? monthTotalHoursWorked { get; set; }
        public decimal? monthTotalHoursWage { get; set; }
    }
}

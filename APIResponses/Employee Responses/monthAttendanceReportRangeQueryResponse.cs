using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.Employee_Responses
{
    public class monthAttendanceReportRangeQueryResponse
    {
        public int? present { get; set; }
        public int? absent { get; set; }
        public int? late { get; set; }
        public int? leave_count { get; set; }
        public int? underTime { get; set; }
        public List<month_attendance_report>months { get; set; }
    }
}

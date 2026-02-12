using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.Employee_Responses
{
    public class yearSummaryAttendanceReportResponse
    {
        public int? year { get; set; }
        public int? present { get; set; }
        public int? late { get; set; }
        public int? underTime { get; set; }
        public decimal? attendanceRate { get; set; }
        public List<month_attendance_report>monthsReport { get; set; }
    }
}

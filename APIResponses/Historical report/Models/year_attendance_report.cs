using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class year_attendance_report
    {
        [Key]
        public int report_id { get; set; }
        public int? year { get; set; }
        public int? present { get; set; }
        public int? average_present { get; set; }
        public int? absent { get; set; }
        public int? average_absent { get; set; }
        public int? late { get; set; }
        public int? average_late { get; set; }
        public int? leave_count { get; set; }
        public int? average_leave { get; set; }
        public int? underTime { get; set; }
        public int? average_undertime { get; set; }
        public decimal? attendanceRate { get; set; }
        public int lastAttendanceIdProcessed { get; set; }
    }

}

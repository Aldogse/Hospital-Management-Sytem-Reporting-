using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class daily_attendance_report
    {
        [Key]
        public int reportId { get; set; }
        public DateTime reportDate {  get; set; }
        public int? present { get; set; }
        public int? absent { get; set; }
        public int? late { get; set; }
        public int? leave { get; set; }
        public int? underTime { get; set; }
        public DateTime lastModifiedDate { get; set; }
        public int lastAttendanceIdProcessed { get; set; }
    }
}

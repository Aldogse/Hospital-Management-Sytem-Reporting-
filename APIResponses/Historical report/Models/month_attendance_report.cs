using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_attendance_report
    {
        [Key]
        public int report_id{ get; set; }
        public DateTime report_date { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int? present { get; set; }
        public int? absent { get; set; }
        public int? late { get; set; }
        public int? leave_count { get; set; }
        public int? underTime { get; set; }
        public DateTime last_modified_date { get; set; }
    }
}

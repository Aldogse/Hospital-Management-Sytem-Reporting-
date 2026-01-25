using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class nurses_month_attendance_report
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public decimal? month_absence_rate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.HR
{
    public class hr_daily_attendance
    {
        [Key]
        public int attendance_id { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        public hr_employees hr_Employees { get; set; }


        public DateOnly attendance_date { get; set; }
        public DateTime? time_in { get; set; }
        public DateTime? time_out { get; set; }
        public decimal? working_hours { get; set; }
        public int late_minutes { get; set; }
        public int undertime_minutes { get; set; }
        public int overtime_minutes { get; set; }
        public attendance_status status { get; set; }
    }
}

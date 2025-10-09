using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_appointment_and_duty_report
    {
        [Key]
        public int reportId { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int? totalAppointments { get; set; }
        public int? doctorDuties { get; set; }
        public int? nurseDuties { get; set; }
        public int? completed { get; set; }
        public int? cancelled { get; set; }
        public int? pending { get; set; }
    }
}

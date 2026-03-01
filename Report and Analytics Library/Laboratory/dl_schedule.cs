using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Laboratory
{
    public class dl_schedule
    {
        [Key]
        public int scheduleID { get; set; }
        public int appointment_id { get; set; }
        public int patientID { get; set; }
        public string? serviceName { get; set; }
        public int employee_id { get; set; }
        public DateTime scheduleDate { get; set; }
        public string? scheduleTime { get; set; }
        public string? status { get; set; }
        public string? notes { get; set; }
        public string? cancel_reason { get; set; }
        public DateTime completed_at { get; set; }
        public int room_id { get; set; }
    }
}

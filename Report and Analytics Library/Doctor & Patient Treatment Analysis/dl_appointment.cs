using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class dl_appointment
    {
        [Key]
        public int appointmentID { get; set; }


        [ForeignKey("patientinfo")]
        public int patientID { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        [ForeignKey("dl_services")]
        public int serviceID { get; set; }
        [JsonIgnore]
        public dl_services dl_services { get; set; }


        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        [JsonIgnore]
        public hr_employees hr_Employees { get; set; }


        public DateOnly appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string? notes { get; set; }
    }
}

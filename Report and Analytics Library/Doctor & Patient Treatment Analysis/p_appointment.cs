using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class p_appointment
    {
        [Key]
        public shift_scheduling appointment_id { get; set; }
        [JsonIgnore]
        public shift_scheduling shift_Scheduling { get; set; }


        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }

        public DateTime appointment_date { get; set; }
        public string? purpose { get; set; }
        public appointmentStatus status { get; set; }
        public string? notes { get; set; }
    }
}

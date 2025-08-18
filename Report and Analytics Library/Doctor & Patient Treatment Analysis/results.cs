using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class results
    {
        [Key]
        public int resultID { get; set; }

        [ForeignKey("Appointment")]
        public int appointmentID { get; set; }
        [JsonIgnore]
        public patient_appointment Appointment { get; set; }

        public DateOnly resultDate { get; set; }
        public string? result { get; set; }
        public string? remarks { get; set; }
    }
}

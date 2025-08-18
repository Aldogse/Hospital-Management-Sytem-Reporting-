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
    public class emr
    {
        [Key]
        public int emr_id { get; set; }

        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        [ForeignKey("appointment")]
        public int? appointment_id { get; set; }
        [JsonIgnore]
        public p_appointment appointment { get; set; }


        public DateTime visit_date { get; set; }
        public string? symptoms { get; set; }
        public string? diagnosis { get; set; }
        public string? treatment { get; set; }
        public string? attending_physician { get; set; }
        public string? notes { get; set; }
    }
}

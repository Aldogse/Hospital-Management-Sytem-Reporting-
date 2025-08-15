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
    public class previous_medical_records
    {
        [Key]
        public int record_id { get; set; }

        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        public string? condition_name { get; set; }
        public DateTime? diagnosis_date { get; set; }
        public string? notes { get; set; }
    }
}

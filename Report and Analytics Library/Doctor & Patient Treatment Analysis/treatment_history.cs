using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Services;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class treatment_history
    {
        [Key]
        public int record_id { get; set; }

        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        [ForeignKey("hospital_Services")]
        public int service_id { get; set; }
        [JsonIgnore]
         public List<hospital_services> hospital_Services { get; set; }


        public DateTime date_used { get; set; }
        public string? attending_staff { get; set; }
        public string? notes { get; set; }
      
    }
}

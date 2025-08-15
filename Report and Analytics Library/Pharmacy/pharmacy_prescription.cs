using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Pharmacy
{
    public class pharmacy_prescription
    {
        [Key]
        public int prescription_id { get; set; }
        public int? doctor_id { get; set; }

        [ForeignKey("patientinfo")]
        public int? patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }

        public DateTime? precription_date { get; set; }
        public string? note { get; set; }
        public string? status { get; set; }

    }
}

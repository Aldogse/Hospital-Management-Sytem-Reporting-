using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;

namespace Report_and_Analytics_Library.Property_Management
{
    public class bed_assignments
    {
        [Key]
        public int assignment_id { get; set; }

        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        [ForeignKey("beds")]
        public int bed_id { get; set; }
        [JsonIgnore]
        public beds beds { get; set; }


        public DateTime assigned_date { get; set; }
        public DateTime? released_date { get; set; }
    }
}

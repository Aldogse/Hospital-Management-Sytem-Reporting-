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
    public class patient_user
    {
        [Key]
        public int user_id { get; set; }

        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public string fname { get; set; }
        public string mname { get; set; }
        public string lname { get; set; }
    }
}

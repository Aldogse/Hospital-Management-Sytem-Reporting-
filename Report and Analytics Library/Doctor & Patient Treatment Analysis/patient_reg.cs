using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Insurance;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class patient_reg
    {
        [Key]
        public int patient_id { get; set; }
        public string name { get; set; }
        public DateOnly birthdate { get; set; }


        [ForeignKey("insurance_Provider")]
        public  int insurance_id { get; set; }
        [JsonIgnore]
        public insurance_provider insurance_Provider { get; set; }

        public DateTime date_registered { get; set; }
    }
}

using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Pharmacy
{
    public class prescriptions
    {
        [Key]
        public int prescription_id { get; set; }

        [ForeignKey("emr")]
        public int emr_id { get; set; }
        public emr emr { get; set; }

        public string? medication_name { get; set; }
        public string? dosage { get; set; }
        public string? frequency { get; set; }
        public string? duration { get; set; }
        public string? instructions { get; set; }
    }
}

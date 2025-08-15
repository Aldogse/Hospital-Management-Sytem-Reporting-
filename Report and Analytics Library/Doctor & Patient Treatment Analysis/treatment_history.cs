using Report_and_Analytics_Library.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class treatment_history
    {
        [Key]
        public int record_id { get; set; }

        [ForeignKey("")]
        public int patient_id { get; set; }

        [ForeignKey("hospital_Services")]
        public int service_id { get; set; }
        public hospital_services hospital_Services { get; set; }


        public DateTime date_used { get; set; }
    }
}

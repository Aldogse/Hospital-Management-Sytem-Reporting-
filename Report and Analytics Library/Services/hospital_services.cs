using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Services
{
    public class hospital_services
    {
        [Key]
        public int service_id { get; set; }
        public string service_name { get; set; }
        public string? description { get; set; }

        public string? department { get; set; }
        public decimal cost { get; set; }
        public List<treatment_history> treatment_Histories { get; set; }

    }
}

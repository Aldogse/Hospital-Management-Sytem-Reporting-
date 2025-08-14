using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class evaluation_criteria_metrics
    {
        [Key]
        public int criterion_id { get; set; }

        public string criterion_name { get; set; }
        public string description { get; set; }
        public double weight_percentage { get; set; }
    }
}

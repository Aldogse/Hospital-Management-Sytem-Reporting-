using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class doctor_evaluation_score
    {
        [Key]
        public int evaluation_score_id { get; set; }
        public int evaluation_id { get; set; }
        public int criterion_id { get; set; }
        public int score { get; set; }
        public int remarks { get; set; }
    }
}

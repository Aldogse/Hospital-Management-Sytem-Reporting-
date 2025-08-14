using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class evaluation_summary_reports
    {
        [Key]
        public int employee_id { get; set; }

        public int average_score { get; set; }
        public string evaluation_period { get; set; }
        public int performance_level { get; set; }
        public int number_of_evaluations { get; set; }
        public DateTime last_evaluated { get; set; }
    }
}

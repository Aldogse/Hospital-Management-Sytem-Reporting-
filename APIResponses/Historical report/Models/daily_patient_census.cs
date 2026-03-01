using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class daily_patient_census
    {
        [Key]
        public int census_id { get; set; }
        public DateOnly census_date { get; set; }
        public int total_admissions { get; set; }
        public int total_discharges { get; set; }
        public int current_patients { get; set; }
        public DateTime generated_at { get; set; }
    }
}

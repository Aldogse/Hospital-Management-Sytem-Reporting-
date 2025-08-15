using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class evaluation_records
    {
        [Key]
        public int evaluation_id { get; set; }

        [ForeignKey("Employee")]
        public int employee_id { get; set; }
        [JsonIgnore]
        public hr_employees Employee { get; set; }

        public int evaluator_id { get; set; }
        public DateOnly evaluation_date { get; set; }
        public int score { get; set; }
        public string rating { get; set; }
        public string comments { get; set; }
        public int status { get; set; }
        public DateTime created_at { get; set; }
    }
}

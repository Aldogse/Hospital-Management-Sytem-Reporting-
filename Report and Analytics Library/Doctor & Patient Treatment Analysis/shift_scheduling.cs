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
    public class shift_scheduling
    {
        [Key]
        public int schedule_id { get; set; }


        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        [JsonIgnore]
        public hr_employees hr_Employees { get; set; }

        public int day_of_week { get; set; }
        public TimeOnly start_time { get; set; }
        public TimeOnly end_time { get; set; }
        public int department_id { get; set; }
        public DateTime created_at { get; set; }
        public List<p_appointment>Appointments { get; set; }
    }
}

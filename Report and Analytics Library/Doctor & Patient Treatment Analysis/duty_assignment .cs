using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class duty_assignment
    {
        [Key]
        public int duty_id { get; set; }


        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        public hr_employees hr_Employees { get; set; }


        public DateOnly operation_date { get; set; }
        public TimeOnly start_time { get; set; }
        public TimeOnly end_time { get; set; }

        public int patient_id { get; set; }


        public string treatment_type { get; set; }
        public string surgery_type { get; set; }
        public string room_location { get; set; }
        public string required_equipment { get; set; }
        public int status { get; set; }
    }
}

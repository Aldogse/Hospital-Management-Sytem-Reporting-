using Report_and_Analytics_Library.HR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class professional_details
    {
        [Key]
        public int employee_id { get; set; }
        public hr_employees hr_Employees { get; set; }

        public int department_id { get; set; }
        public string specialization { get; set; }
        public string position_title { get; set; }
        public string shift_type { get; set; }
        public string employment_type { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class doctor_duty_update
    {
        [Key]
        public int update_id { get; set; }
        public int MyProperty { get; set; }
    }
}

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

        public int appointment_id { get; set; }
        public int doctor_id { get; set; }
        public int? bed_id { get; set; }
        public int? nurse_assistant { get; set; }
        public string procedure { get; set; }
        public string? equipment { get; set; }
        public string? tools { get; set; }
        public string notes { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}

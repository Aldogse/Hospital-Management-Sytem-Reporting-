using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class patientinfo
    {
        [Key]
        public int patient_id { get; set; }
        public string fname { get; set; }
        public string? mname { get; set; }
        public string lname { get; set; }
        public string address { get; set; }
        public int age { get; set; }
        public DateOnly dob { get; set; }
        public string gender { get; set; }
        public string civil_status { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public string admission_type { get; set; }
        public int bed_number { get; set; }
        public int attending_doctor { get; set; }
    }
}

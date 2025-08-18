using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class users
    {
        [Key]
        public int user_id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string fname { get; set; }
        public string mname { get; set; }
        public string lname { get; set; }
    }
}

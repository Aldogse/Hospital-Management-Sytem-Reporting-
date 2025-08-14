using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Property_Management
{
    public class bed_assignments
    {
        [Key]
        public int assignment_id { get; set; }

        [ForeignKey("")]
        public int patient_id { get; set; }

        [ForeignKey("")]
        public int bed_id { get; set; }

        public DateTime assigned_date { get; set; }
        public DateTime? released_date { get; set; }
    }
}

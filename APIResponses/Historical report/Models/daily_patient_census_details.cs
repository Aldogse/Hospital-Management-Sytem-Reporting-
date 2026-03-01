using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class daily_patient_census_details
    {
        [Key]
        public int detail_id { get; set; }
        public string census_id { get; set; }
        public int patient_id { get; set; }
        public string full_name { get; set; }
        public int bed_id { get; set; }
        public DateOnly assigned_date { get; set; }
        public DateOnly released_date { get; set; }
        public string condition_name { get; set; }
    }
}

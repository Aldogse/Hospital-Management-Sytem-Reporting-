using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_admission_and_discharge_report
    {
        [Key]
        public int report_id { get; set; }
        public int total_beds { get; set; }
        public int occupied_beds { get; set; }
        public int available_beds { get; set; }
        public int recently_discharged { get; set; }
        public int broken_beds { get; set; }
        public int month{ get; set; }
        public int year { get; set; }
    }
}

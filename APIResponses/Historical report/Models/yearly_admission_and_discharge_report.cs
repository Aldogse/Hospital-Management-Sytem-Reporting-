using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class yearly_admission_and_discharge_report
    {
        [Key]
        public int report_id { get; set; }
        public int year { get; set; }
        public double occupied_beds { get; set; }
        public double available_beds { get; set; }
        public double broken_beds { get; set; }
        public int total_beds { get; set; }
      
    }
}

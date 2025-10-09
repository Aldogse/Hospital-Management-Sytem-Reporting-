using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_appointments
    {
        [Key]
        public int reportId { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int doctor_id { get; set; }
        public int? bed_id { get; set; }
        public int nurse_id { get; set; }
        public string procedure { get; set; }
        public string status { get; set; }
    }
    
}

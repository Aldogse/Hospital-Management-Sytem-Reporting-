using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class dl_services
    {
        [Key]
        public int serviceID { get; set; }
        public string serviceName { get; set; }
        public string? description { get; set; }
        public decimal price { get; set; }
        public int durationMinutes { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Insurance
{
    public class insurance_request
    {
        [Key]
        public int request_id { get; set; }
        public int patient_id { get; set; }
        public int billing_id { get; set; }
        public string insurance_type { get; set; }
        public decimal insurance_covered { get; set; }
        public string notes { get; set; }
        public string status { get; set; }
        public int insurance_number { get; set; }
    }
}

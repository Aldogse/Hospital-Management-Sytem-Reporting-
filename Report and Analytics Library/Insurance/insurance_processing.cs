using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Insurance
{
    public class insurance_processing
    {
        [Key]
        public int insurance_id { get; set; }
        public string provider_name { get; set; }
        public string? contact_email { get; set; }
        public string? contact_number { get; set; }
        public decimal coverage_percent { get; set; }
    }
}

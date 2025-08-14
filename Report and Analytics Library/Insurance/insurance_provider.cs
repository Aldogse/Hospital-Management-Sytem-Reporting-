using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Insurance
{
    public class insurance_provider
    {
        [Key]
        public int insurance_provider_id { get; set; }

        public string? name { get; set; }
        public string? contact_person { get; set; }
        public string? contact_number { get; set; }
    }
}

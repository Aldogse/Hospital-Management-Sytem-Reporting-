using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Billing
{
    public class biiling_service
    {
        [Key]
        public int service_id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public bool is_insurance_eligible { get; set; }
    }
}

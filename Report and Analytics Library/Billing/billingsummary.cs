using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Billing
{
    public class billingsummary
    {
        [Key]
        public int summary_id { get; set; }
        public string particulars { get; set; }
        public decimal actual_charges { get; set; }
        public decimal vat { get; set; }
        public decimal amount_of_discount { get; set; }
        public decimal out_of_pocket { get; set; }
    }
}

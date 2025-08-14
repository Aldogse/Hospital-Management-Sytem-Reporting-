using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.Billing
{
    public class bills
    {
        [Key]
        public int bill_id { get; set; }

        public int patient_id { get; set; }

        public decimal total_amount { get; set; }
        public decimal balance_due { get; set; }
        public billingStatus status { get; set; }
    }
}

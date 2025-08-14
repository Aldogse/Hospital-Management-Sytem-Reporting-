using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Billing;

namespace Report_and_Analytics_Library.Insurance
{
    public class insurance_coverage_rules
    {
        [Key]
        public int insurance_coverage_id { get; set; }


        [ForeignKey("insurance_Provider")]
        public int insurance_provider_id { get; set; }
        public insurance_provider insurance_Provider { get; set; }


        [ForeignKey("biiling_Service")]
        public int? service_id { get; set; }
        public biiling_service biiling_Service { get; set; }

        public bool is_covered { get; set; }
    }
}

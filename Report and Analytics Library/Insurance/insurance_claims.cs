using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Billing;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.Insurance
{
    public class insurance_claims
    {
        [Key]
        public int insurance_claims_id { get; set; }

        [ForeignKey("bills")]
        public int bill_id { get; set; }
        [JsonIgnore]
        public bills bills { get; set; }

        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        [ForeignKey("insurance_Provider")]
        public int insurance_provider_id { get; set; }
        [JsonIgnore]
        public insurance_provider insurance_Provider { get; set; }

        public decimal claim_amount { get; set; }
        public claimStatus status { get; set; }
        public DateOnly submmited_date { get; set; }
        public DateOnly resolved_date { get; set; }
    }
}

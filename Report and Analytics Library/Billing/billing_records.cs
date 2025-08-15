using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.Billing
{
    public class billing_records
    {
        [Key]
        public int billing_id { get; set; }


        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        public DateTime billing_date { get; set; }
        public decimal? total_amount { get; set; }
        public decimal insurance_covered { get; set; }
        public decimal out_of_pocket { get; set; }
        public billingStatus status { get; set; }
        public string? payment_method { get; set; }
        public string? transaction_id { get; set; }
    }
}

using Org.BouncyCastle.Asn1.Crmf;
using Report_and_Analytics_Library.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Report_and_Analytics_Library.Billing
{
    public class payment_transaction
    {
        [Key]
        public int transaction_id { get; set; }


        [ForeignKey("billing_Records")]
        public int? billing_id { get; set; }
        [JsonIgnore]
        public billing_records? billing_Records { get; set; }

        public string? gateway_name { get; set; }
        public payment_status? payment_status { get; set; }
        public decimal? paid_amount { get; set; }
        public string? reference_code { get; set; }
        public DateTime transaction_date { get; set; }
        public string? paid_by{ get; set; }
        public string? receipt_number { get; set; }


    }
}

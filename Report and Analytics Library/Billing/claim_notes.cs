using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Insurance;

namespace Report_and_Analytics_Library.Billing
{
    public class claim_notes
    {
        [Key]
        public int claim_note_id { get; set; }


        [ForeignKey("insurance_Claims")]
        public int insurance_claims_id { get; set; }
        [JsonIgnore]
        public insurance_claims insurance_Claims { get; set; }


        public string note { get; set; }
        public string added_by { get; set; }
    }
}

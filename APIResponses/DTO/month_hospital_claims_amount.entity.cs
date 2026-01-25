using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_hospital_claims_amount_entity
    {
        public float insurance_provider_id { get; set; }
        public float month { get; set; }
        public float year { get; set; }

        //TARGET VALUES
        public float total_claim_amount_submitted { get; set; }
        public float total_claim_approved_amount { get; set; }
        public float total_claim_declined_amount { get; set; }

        public float last_month_total_claim_approved_amount { get; set; }
        public float last_month_total_claim_declined_amount { get; set; }
    }
}

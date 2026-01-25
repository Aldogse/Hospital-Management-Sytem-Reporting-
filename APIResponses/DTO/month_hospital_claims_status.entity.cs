using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_hospital_claims_status_entity
    {
        public float insurance_provider_id { get; set; }
        public float month { get; set; }
        public float year { get; set; }

        public float total_claims { get; set; }
        public float last_month_approved_claims { get; set; }
        public float last_month_denied_claims { get; set; }

        
        public float total_claim_approved { get; set; }
        public float total_claim_denied { get; set; }
    }
}

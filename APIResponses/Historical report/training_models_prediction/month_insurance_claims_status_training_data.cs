using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.training_models_prediction
{
    public class month_insurance_claims_status_training_data
    {
        [Key]
        public int training_id { get; set; }
        public int insurance_provider_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        // DERIVED VALUES
        public int total_claims { get; set; }
        public int last_month_approved_claims { get; set; }
        public int last_month_denied_claims { get; set; }

        // TARGET VALUES
        public int? total_claim_approved { get; set; }
        public int? total_claim_denied { get; set; }

    }
}

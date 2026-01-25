using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace APIResponses.forecast
{
    public class monthNumberOfClaimPrediction
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }
        public float total_claims { get; set; }
        public float total_claim_approved { get; set; }
        public float total_claim_denied { get; set; }
    }
}

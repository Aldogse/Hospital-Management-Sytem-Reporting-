using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace APIResponses.forecast
{
    public class monthClaimAmountPrediction
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }
        public float total_claim_approved_amount { get; set; }
        public float total_claim_declined_amount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace APIResponses.forecast
{
    public class monthRevenueForecast
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }
        public float total_revenue { get; set; }
        public float pharmacy_total_transactions { get; set; }
        public float average_bill_amount { get; set; }
        public float total_patient {  get; set; }
    }
}

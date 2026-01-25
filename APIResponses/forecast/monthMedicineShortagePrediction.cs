using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace APIResponses.forecast
{
    public class monthMedicineShortagePrediction
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }

        public float avg_daily_use { get; set; }
        //data to be predicted
        public float shortage_occured { get; set; }
    }
}

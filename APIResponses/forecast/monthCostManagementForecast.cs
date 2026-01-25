using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace APIResponses.forecast
{
    public class monthCostManagementForecast
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }
        public float[] month_forecasted_cost { get; set; }
    }
}

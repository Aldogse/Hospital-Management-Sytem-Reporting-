using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace APIResponses.forecast
{
    public class monthStaffingNeedsForecast
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }
        public float total_working_hours_needed { get; set; } // label or feature
        public float total_staff_needed { get; set; }
    }
}

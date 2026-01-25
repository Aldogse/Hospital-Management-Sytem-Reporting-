using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace APIResponses.forecast
{
    public class monthBedOccupancyForecast
    {
        [ColumnName("Score")]
        public float[] score {  get; set; }

        public float PredictedOccupiedBeds { get; set; }
        public float PredictedRecentlyDischarged {  get; set; }
        public float PredictedBedOccupancyRate { get; set; }
        public float PredictedBrokenBedRate {  get; set; }
    }
}

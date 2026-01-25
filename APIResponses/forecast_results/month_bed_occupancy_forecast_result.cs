using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.forecast_results
{
    public class month_bed_occupancy_forecast_result
    {
        [Key]
        public int result_id { get; set; }
        
        public int month { get; set; }
        public int year { get; set; }

        //results
        public int predicted_occupied_beds {  get; set; }
        public int predicted_recently_discharged { get; set; }
        public float predicted_bed_occupancy_rate { get; set; }
        public float predicted_broken_bed_rate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_bed_occupancy_training_entity
    {
        public float month { get; set; }
        public float year { get; set; }
        public float total_beds { get; set; }
        public float occupied_beds { get; set; }
        public float recently_discharged { get; set; }
        public float bed_occupancy_rate { get; set; }
        public float broken_bed_rate { get; set; }
    }
}

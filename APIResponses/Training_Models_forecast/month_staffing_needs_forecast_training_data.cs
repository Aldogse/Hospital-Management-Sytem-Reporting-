using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlX.XDevAPI;

namespace APIResponses.Training_Models_forecast
{
    public class month_staffing_needs_forecast_training_data
    {
        [Key]
        public int training_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        public string department { get; set; }
        public decimal avg_staff_present { get; set; }
        public decimal avg_working_hours { get; set; }
        public decimal avg_overtime_hours { get; set; }

        public decimal total_working_hours_needed { get; set; }
        public decimal total_staff_needed { get; set; }
    }
}

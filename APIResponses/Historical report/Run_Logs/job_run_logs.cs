using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Run_Logs
{
    public class job_run_logs
    {
        [Key]
        public string job_name { get; set; }
        public int run_year { get; set; }
        public DateTime last_run { get; set; }
    }
}

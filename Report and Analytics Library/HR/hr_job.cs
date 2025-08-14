using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class hr_job
    {
        [Key]
        public int? job_id { get; set; }
        public string? title { get; set; }
        public string? job_position { get; set; }
        public string? job_description { get; set; }
        public string? specialization { get; set; }
        public DateTime? date_post { get; set; }
        public string? image { get; set; }
    }
}

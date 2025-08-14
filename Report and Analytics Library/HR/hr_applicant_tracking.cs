using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class hr_applicant_tracking
    {
        [Key]
        public int tracking_id { get; set; }

        [ForeignKey("Hr_Application")]
        public int? applicant_id { get; set; }
        public hr_application Hr_Application { get; set; }

        public string? status { get; set; }
        public DateTime? interview_date { get; set; }
        public string? interview_status { get; set; }
        public string? notes { get; set; }
        public DateTime update_at { get; set; }
    }
}

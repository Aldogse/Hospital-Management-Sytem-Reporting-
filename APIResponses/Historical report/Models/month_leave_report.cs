using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_leave_report
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int total_leave_request { get; set; }
        public int? month_pending_leaves { get; set; }
        public int? month_approved_leaves { get; set; }
        public  int? month_rejected_leaves { get; set; }
    }
}

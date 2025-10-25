using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.HR;

namespace APIResponses.Historical_report
{
    public class monthLeaveReportResponse
    {
        [Key]
        public int reportId { get; set; }
        public int? total_leaves { get; set; }
        public int? approved { get; set; }
        public int? rejected { get; set; }
        public int? pending { get; set; }
        public List<hr_leave> leaves { get; set; }
    }
}

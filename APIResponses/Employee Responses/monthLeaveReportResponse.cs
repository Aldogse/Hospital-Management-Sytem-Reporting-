using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report
{
    public class monthLeaveReportResponse
    {
        [Key]
        public int reportId { get; set; }
        public DateOnly date { get; set; }
        public int approved { get; set; }
        public int rejected { get; set; }
        public int pending { get; set; }
    }
}

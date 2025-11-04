using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PropertyAndManagementResponse
{
    public class monthDischargeReportResponse
    {
        public int bedId { get; set; }
        public string? ward { get; set; }
        public string? roomNumber { get; set; }
        public string? bedType { get; set; }
        public DateTime? releasedDate { get; set; }
    }
}

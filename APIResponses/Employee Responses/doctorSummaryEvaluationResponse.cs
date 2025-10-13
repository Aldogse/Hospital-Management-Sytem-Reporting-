using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Employee_Responses
{
    public class doctorSummaryEvaluationResponse
    {
        public string created_at { get; set; }
        public int score { get; set; }
        public string rating { get; set; }
        public string comments { get; set; }
    }
}

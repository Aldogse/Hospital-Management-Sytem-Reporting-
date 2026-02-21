using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.BudgetResponse
{
    public class monthPendingBudgetsReport
    {
        public string month { get; set; }
        public decimal? requested_amount { get; set; }
        public decimal? allocated_budget { get; set; }
        public decimal? approved_amount { get; set; }
        public string status { get; set; }
        public DateTime request_date { get; set; }
    }
}

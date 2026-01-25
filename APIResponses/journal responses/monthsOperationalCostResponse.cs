using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class monthsOperationalCostResponse
    {
        public int month { get; set; }
        public int year { get; set; }

        //TARGET VALUE
        public float total_month_operational_cost { get; set; }
    }
}

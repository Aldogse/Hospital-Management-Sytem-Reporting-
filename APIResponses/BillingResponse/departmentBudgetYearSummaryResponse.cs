using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Property_Management;

namespace APIResponses.BillingResponse
{
    public class departmentBudgetYearSummaryResponse
    {
        public decimal? totalAllocated { get; set; }
        public decimal? totalRequested { get; set; }
        public decimal? totalApproved { get; set; }
        public List<department_budgets> budgets { get; set; }
    }
}

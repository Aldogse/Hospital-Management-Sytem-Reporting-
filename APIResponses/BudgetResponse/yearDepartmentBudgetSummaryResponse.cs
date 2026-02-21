using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Property_Management;

namespace APIResponses.BudgetResponse
{
    public class yearDepartmentBudgetSummaryResponse
    {
        public int year { get; set; }
        public decimal? total_allocated { get; set; }
        public decimal? total_requested { get; set; }
        public decimal? total_approved { get; set; }
        public List<department_budgets> monthBudgetsReport { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Property_Management
{
    public class department_budgets
    {
        [Key]
        public int budget_id { get; set; }
        public int user_id { get; set; }
        public string month { get; set; }
        public decimal requested_amount { get; set; }
        public decimal allocated_budget { get; set; }
        public decimal approved_amount { get; set; }
        public string status { get; set; }
        public DateTime request_date { get; set; }
    }
}

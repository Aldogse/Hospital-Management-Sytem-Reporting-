using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class expense_logs
    {
        [Key]
        public int expense_id { get; set; }
        public string? category { get; set; }
        public string? description { get; set; }
        public decimal amount { get; set; }
        public DateOnly? expense_date { get; set; }
        public string recorded_by { get; set; }
    }
}

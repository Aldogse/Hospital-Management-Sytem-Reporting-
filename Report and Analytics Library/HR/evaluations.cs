using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class evaluations
    {
        [Key]
        public int evaluation_id { get; set; }
        public int evaluatee_id  { get; set; }
        public int evaluator_id { get; set; }
        public string comments { get; set; }
        public decimal? average_score { get; set; }
        public string? performance_level { get; set; }
        public int total_score { get; set; }
        public DateOnly evaluation_date { get; set; }
        public string? ai_feedback { get; set; }
    }
}

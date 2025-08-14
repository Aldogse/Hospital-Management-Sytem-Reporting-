using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.HR
{
    public class hr_payroll_audit_log
    {
        [Key]
        public int audit_id { get; set; }

        [ForeignKey("hr_payroll")]
        public int? payroll_id { get; set; }
        public hr_payroll hr_payroll { get; set; }

        public string processed_by { get; set; }
        public payroll_action_log? action { get; set; }
        public string? remarks { get; set; }
        public DateTime timestamp { get; set; }
    }
}

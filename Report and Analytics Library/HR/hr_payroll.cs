using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.HR
{
    public class hr_payroll
    {
        [Key]
        public int payroll_id { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        [JsonIgnore]
        public hr_employees hr_Employees { get; set; }

        public DateOnly pay_period_start { get; set; }
        public DateOnly? pay_period_end { get; set; }
        public int days_worked { get; set; }
        public decimal overtime_hours { get; set; }
        public decimal? basic_pay { get; set; }
        public decimal? overtime_pay { get; set; }
        public decimal allowances { get; set; }
        public decimal bonuses { get; set; }
        public decimal thirteenth_month { get; set; }
        public decimal sss_deduction { get; set; }
        public decimal philhealth_deduction { get; set; }
        public decimal pagibig_deduction { get; set; }
        public decimal loan_deduction { get; set; }
        public decimal absence_deduction { get; set; }
        public decimal gross_pay { get; set; }
        public decimal? total_deductions { get; set; }
        public decimal? net_pay { get; set; }
        public string disbursement_method { get; set; }
        public string status { get; set; }
        public DateTime date_generated { get; set; }
        public List<hr_payroll_audit_log> hr_Payroll_Audit_Logs { get; set; }
        public List<hr_payroll_disbursement> hr_Payroll_Disbursements { get; set; }
        public List<hr_payslips> hr_Payslips { get; set; }
    }
}

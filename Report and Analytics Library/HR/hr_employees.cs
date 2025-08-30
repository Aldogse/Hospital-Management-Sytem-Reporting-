using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Report_and_Analytics_Library.HR
{
    public class hr_employees
    {
        [Key]
        public int employee_id { get; set; }
        public string? name { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? first_name { get; set; }
        public string? middle_name { get; set; }
        public string? last_name { get; set; }
        public string? suffix_name { get; set; }
        public string? gender { get; set; }
        public DateOnly? date_of_birth { get; set; }
        public string? contact_number { get; set; }
        public string? email { get; set; }
        public string? citizenship { get; set; }
        public string? house_no { get; set; }
        public string? barangay { get; set; }
        public string? city { get; set; }
        public string? province { get; set; }
        public string? region { get; set; }
        public DateOnly? hire_date { get; set; }
        public string? profession { get; set; }
        public string? role { get; set; }
        public string? department { get; set; }
        public string? specialization { get; set; }
        public string? employment_type { get; set; }
        public string status { get; set; }
        public string? educational_status { get; set; }
        public string? degree_type { get; set; }
        public string? medical_school { get; set; }
        public int? graduation_year { get; set; }
        public string? license_type { get; set; }
        public string? license_number { get; set; }
        public DateOnly? license_issued { get; set; }
        public DateOnly?  license_expiry { get; set; }
        public string? eg_name { get; set; }
        public string? eg_relationship { get; set; }
        public string? eg_cn { get; set; }
        public int leave_credits { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? update_at { get; set; }
        public ICollection<hr_attendance_flags> attendance_Flags { get; set; }
        public ICollection<hr_leave> hr_Leaves { get; set; }
        public ICollection<hr_payroll> hr_Payrolls { get; set; }
        public ICollection<hr_payroll_disbursement> hr_Payroll_Disbursements { get; set; }
        public ICollection<hr_payslips> hr_Payslips { get; set; }
    }
}

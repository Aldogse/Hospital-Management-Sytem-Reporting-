using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class hr_payslips
    {
        [Key]
        public int payslip_id { get; set; }

        [ForeignKey("hr_Payroll")]
        public int payroll_id { get; set; }
        public hr_payroll hr_Payroll { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        public hr_employees hr_Employees { get; set; }

        public string? payslip_pdf_path { get; set; }
        public DateTime date_issued { get; set; }
    }
}

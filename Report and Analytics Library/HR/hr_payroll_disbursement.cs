using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class hr_payroll_disbursement
    {
        [Key]
        public int disbursement_id { get; set; }

        [ForeignKey("hr_Payroll")]
        public int payroll_id { get; set; }
        public hr_payroll hr_Payroll { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        public hr_employees hr_Employees { get; set; }

        public string? reference_no { get; set; }
        public string? disbursed_by { get; set; }
        public DateTime disbursement_date { get; set; }
    }
}

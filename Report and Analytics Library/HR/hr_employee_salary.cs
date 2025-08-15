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
    public class hr_employee_salary
    {
        [Key]
        public int salary_id { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        [JsonIgnore]
        public hr_employees hr_Employees { get; set; }

        public decimal? daily_rate { get; set; }
        public pay_type pay_type { get; set; }
        public bool has_sss { get; set; }
        public bool has_philhealth { get; set; }
        public bool has_pagibig { get; set; }
        public bool has_loan { get; set; }
        public decimal loan_amount { get; set;}
    }
}

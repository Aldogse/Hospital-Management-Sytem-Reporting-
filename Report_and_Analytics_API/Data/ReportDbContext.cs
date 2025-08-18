using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_Library.Billing;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using Report_and_Analytics_Library.Enums;
using Report_and_Analytics_Library.HR;
using Report_and_Analytics_Library.Insurance;
using Report_and_Analytics_Library.JournalEntry;
using Report_and_Analytics_Library.Pharmacy;
using Report_and_Analytics_Library.Property_Management;
using Report_and_Analytics_Library.Services;

namespace Report_and_Analytics_API.Data
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
        {

        }
        //Billing tables
        public DbSet<biiling_service> biiling_service { get; set; }   
        public DbSet<bill_items> bill_Items { get; set; }
        public DbSet<billing_records> billing_Items { get; set; }
        public DbSet<billing_records> billing_records { get; set; }
        public DbSet<billingsummary> billingsummary { get; set; }
        public DbSet<bills> bills { get; set; }
        public DbSet<claim_notes> claim_notes { get; set; }
        public DbSet<compliance_licensing> compliance_licensing { get; set; }
        public DbSet<payment_transaction> payment_transaction { get; set; }


        //Doctor and Patient Treatment Analysis Tables
        public DbSet<dl_appointment> dl_appointments { get; set; }
        public DbSet<dl_services> dl_services { get; set; }
        public DbSet<doctor_evaluation_score> doctor_evaluation_score { get; set; }
        public DbSet<duty_assignment> duty_assignment { get; set; }
        public DbSet<emr> emr { get; set; }
        public DbSet<evaluation_criteria_metrics> evaluation_criteria_metrics { get; set; }
        public DbSet<evaluation_records> evaluation_records { get; set; }
        public DbSet<evaluation_summary_reports> evaluation_summary_reports { get; set; }
        public DbSet<expense_logs> expense_logs { get; set; }
        public DbSet<p_appointment> p_appointment { get; set; }
        public DbSet<patient_receivables> patient_receivables { get; set; }
        public DbSet<patient_reg> patient_reg { get; set; }
        public DbSet<patient_user> patient_user { get; set; }
        public DbSet<patientinfo> patientinfo { get; set; }
        public DbSet<previous_medical_records> previous_medical_records { get; set; }
        public DbSet<professional_details> professional_details { get; set; }
        public DbSet<results> results { get; set; }
        public DbSet<shift_scheduling> shift_scheduling { get; set; }
        public DbSet<treatment_history> treatment_history { get; set; }
        public DbSet<users> users { get; set; }


        //HR related tables
        public DbSet<hr_applicant_documents> hr_Applicant_Documents { get; set; }
        public DbSet<hr_application> hr_application { get; set; }
        public DbSet<hr_attendance_flags> hr_attendance_flags { get; set; }
        public DbSet<hr_attendance_logs> hr_attendance_logs { get; set; }
        public DbSet<hr_daily_attendance> hr_daily_attendance { get; set; }
        public DbSet<hr_employee_salary> hr_employee_salary { get; set; }
        public DbSet<hr_employees> hr_employees { get; set; }
        public DbSet<hr_employees_documents> hr_employees_documents { get; set; }
        public DbSet<hr_job> hr_job { get; set; }
        public DbSet<hr_leave> hr_leave { get; set; }
        public DbSet<hr_payroll> hr_payroll { get; set; }
        public DbSet<hr_payroll_audit_log> hr_payroll_audit_log { get; set; }
        public DbSet<hr_payroll_disbursement> hr_payroll_disbursement { get; set; }
        public DbSet<hr_payslips> hr_payslips { get; set; }

        //Insurance table
        public DbSet<insurance_claims> insurance_claims { get; set; }
        public DbSet<insurance_coverage_rules> insurance_coverage_rules { get; set; }
        public DbSet<insurance_processing> insurance_processing { get; set; }
        public DbSet<insurance_provider> insurance_provider { get; set; }

        //Journal Entry Table
        public DbSet<journal_account> journal_account { get; set; }
        public DbSet<journal_entry> journal_entry { get; set; }
        public DbSet<journal_entry_line> journal_entry_line { get; set; }
        public DbSet<license_expiry> license_expiry { get; set; }

        //pharmacy tables
        public DbSet<pharmacy_inventory> pharmacy_inventory { get; set; }
        public DbSet<pharmacy_prescription> pharmacy_prescription { get; set; }
        public DbSet<pharmacy_prescription_items> pharmacy_prescription_items { get; set; }
        public DbSet<pharmacy_sales> pharmacy_sales { get; set; }
        public DbSet<prescriptions> prescriptions { get; set; }

        //Property management table
        public DbSet<bed_assignments> bed_assignments { get; set; }
        public DbSet<beds> beds { get; set; }

        //Services  table
        public DbSet<hospital_services> hospital_services { get; set; }
        public DbSet<services> services { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<hr_applicant_documents>()
                .HasOne(i => i.Hr_Application)
                .WithMany(i => i.Documents)
                .HasForeignKey(i => i.applicant_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<hr_attendance_flags>()
                .HasOne(i => i.hr_Employees)
                .WithMany(i => i.attendance_Flags)
                .HasForeignKey(i => i.employee_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

                     



   

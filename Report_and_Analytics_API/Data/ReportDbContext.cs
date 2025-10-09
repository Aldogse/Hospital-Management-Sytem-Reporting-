using APIResponses.Historical_report;
using APIResponses.Historical_report.Models;
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


        //Doctor and Patient Treatment Analysis Tables
        public DbSet<dl_services> dl_services { get; set; }
        public DbSet<doctor_evaluation_score> doctor_evaluation_score { get; set; }
        public DbSet<duty_assignment> duty_assignments { get; set; }
        public DbSet<p_emr> p_emr { get; set; }
        public DbSet<evaluation_criteria_metrics> evaluation_criteria_metrics { get; set; }
        public DbSet<evaluation_records> evaluation_records { get; set; }
        public DbSet<evaluation_summary_reports> evaluation_summary_reports { get; set; }
        public DbSet<expense_logs> expense_logs { get; set; }
        public DbSet<p_appointments> p_appointments { get; set; }
        public DbSet<patient_receivables> patient_receivables { get; set; }
        public DbSet<patient_reg> patient_reg { get; set; }
        public DbSet<patient_user> patient_user { get; set; }
        public DbSet<patientinfo> patientinfo { get; set; }
        public DbSet<p_previous_medical_records> p_previous_medical_records { get; set; }
        public DbSet<professional_details> professional_details { get; set; }
        public DbSet<shift_scheduling> shift_scheduling { get; set; }
        public DbSet<p_treatment_history> p_treatment_history { get; set; }
        public DbSet<users> users { get; set; }


        //HR related tables
        public DbSet<hr_applicant_documents> hr_Applicant_Documents { get; set; }
        public DbSet<hr_daily_attendance> hr_daily_attendance { get; set; }
        public DbSet<hr_employee_salary> hr_employee_salary { get; set; }
        public DbSet<hr_employees> hr_employees { get; set; }
        public DbSet<hr_employees_documents> hr_employees_documents { get; set; }
        public DbSet<hr_job> hr_job { get; set; }
        public DbSet<hr_leave> hr_leave { get; set; }
        public DbSet<hr_payroll> hr_payroll { get; set; }
        public DbSet<hr_payroll_audit_log> hr_payroll_audit_log { get; set; }

        //Insurance table
        public DbSet<insurance_claims> insurance_claims { get; set; }
        public DbSet<insurance_coverage_rules> insurance_coverage_rules { get; set; }
        public DbSet<insurance_processing> insurance_processing { get; set; }
        public DbSet<insurance_provider> insurance_provider { get; set; }
        public DbSet<insurance_request> insurance_request { get; set; }
        public DbSet<insurance_logs> insurance_logs { get; set; }

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
        public DbSet<p_prescriptions> p_prescriptions { get; set; }

        //Property management table
        public DbSet<p_bed_assignments> p_bed_assignments { get; set; }
        public DbSet<p_beds> p_beds { get; set; }

        //Services  table
        public DbSet<p_hospital_services> p_hospital_services { get; set; }
        public DbSet<services> services { get; set; }

        //historical db
        public DbSet<employeeReportFinalData> payrollinformation { get; set; }
        public DbSet<employeeAnnualPayrollReport> employeeAnnualPayrollReports { get; set; }
        public DbSet<employeePayrollMonthReport> employeePayrollMonthReports { get; set; }
        public DbSet<month_revenue_breakdownreport> month_revenue_breakdownreport { get; set; }
        public DbSet<monthly_revenue_report> monthly_revenue_report { get; set; }
        public DbSet<quarter_revenue> quarter_revenue { get; set; }
        public DbSet<monthly_claim_report> monthly_claim_report { get; set; }
        public DbSet<daily_attendance_report> daily_attendance_report { get; set; }
        public DbSet<month_appointment_and_duty_report> month_appointment_and_duty_report { get; set; }
        public DbSet<quarter_employees_performance_and_evaluation_report> quarter_employees_performance_and_evaluation_report { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<hr_leave>().
                HasOne(i => i.hr_Employees)
                .WithMany(i => i.hr_Leaves)
                .HasForeignKey(i => i.employee_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<hr_payroll>()
                .HasOne(i => i.hr_Employees)
                .WithMany(i => i.hr_Payrolls)
                .HasForeignKey(i => i.employee_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<hr_payroll_audit_log>()
                .HasOne(i => i.hr_payroll)
                .WithMany(i => i.hr_Payroll_Audit_Logs)
                .HasForeignKey(i => i.payroll_id)
                .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<hr_payroll_disbursement>()
            //    .HasOne(i => hr_payroll)
            //    .WithMany(i => hr_payroll_disbursement)
            //    .HasForeignKey(i => i.payroll_id)
            //    .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<p_appointments>()
                .HasOne(i => i.patientinfo)
                .WithMany(i => i.p_Appointments)
                .HasForeignKey(i => i.appointment_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<p_bed_assignments>()
                .HasOne(i => i.patientinfo)
                .WithMany(i => i.bed_Assignments)
                .HasForeignKey(i => i.patient_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<p_bed_assignments>()
               .HasOne(i => i.beds)
               .WithMany(i => i.bed_Assignments)
               .HasForeignKey(i => i.bed_id)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<p_emr>()
                .HasOne(i => i.patientinfo)
                .WithMany(i => i.emr)
                .HasForeignKey(i => i.patient_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<p_emr>()
                .HasOne(i => i.appointment)
                .WithMany(i => i.emrs)
                .HasForeignKey(i => i.appointment_id)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<p_prescriptions>()
                .HasOne(i => i.emr)
                .WithMany(i => i.prescriptions)
                .HasForeignKey(i => i.emr_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<p_treatment_history>()
               .HasOne(i => i.patientinfo)
               .WithMany(i => i.treatment_Histories)
               .HasForeignKey(i => i.patient_id)
               .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<p_treatment_history>()
               .HasOne(i => i.hospital_Services)
               .WithMany(i => i.treatment_Histories)
               .HasForeignKey(i => i.service_id)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<employeeReportFinalData>(entity =>
            {
                entity.Property(e => e.overtimePay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleGrossPay).HasPrecision(18, 2);
                entity.Property(e => e.GrossPay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleTotalDeductions).HasPrecision(18, 2);
                entity.Property(e => e.ytdTotalDeductions).HasPrecision(18, 2);
                entity.Property(e => e.ytdNetPay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleNetpay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleSssDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdsssDeductions).HasPrecision(18, 2);
                entity.Property(e => e.payCyclePhilHealthDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdphilHealthDeductions).HasPrecision(18, 2);
                entity.Property(e => e.payCycleLoanDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdLoanDeductions).HasPrecision(18, 2);
                entity.Property(e => e.payCycleAbsenceDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdAbsenceDeductions).HasPrecision(18, 2);
            });

        }
    }
}

                     



   

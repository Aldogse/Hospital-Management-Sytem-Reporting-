
using System.Runtime.InteropServices;
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Service
{
    public class monthShiftAndDutiesReport : BackgroundService
    {
        private readonly ILogger<monthShiftAndDutiesReport> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthShiftAndDutiesReport(ILogger<monthShiftAndDutiesReport>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceScope.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

            if(DateTime.Now.Day == 7)
            {
                await getMonthShiftAndDutiesReport(dbContext);
                await Task.Delay(TimeSpan.FromDays(1));
            }

            //USED TO AVOID RUNNING THE SERVICE TWICE
            await Task.Delay(TimeSpan.FromDays(1));
        }


        //FUNCTION THAT RUNS EVERY MONTH TO CHECK PREV MONTH  REPORT FOR 
        //DUTIES AND SHIFT
        private async Task getMonthShiftAndDutiesReport(ReportDbContext reportDbContext)
        {
            
            var prevMonth = DateTime.Now.AddMonths(-1);

            _logger.LogInformation($"Extraction begins for {prevMonth.Month} / {prevMonth.Year}");
            try
            {
                //will populate month appointment duty and report
                var monthAppointmentsAndDutiesReport = await (
                    from appointment in reportDbContext.p_appointments
                    join duty in reportDbContext.duty_assignments
                    on appointment.appointment_id equals duty.appointment_id
                    where appointment.appointment_date.Month == prevMonth.Month
                    && appointment.appointment_date.Year == prevMonth.Year
                    group new { appointment , duty} by 1 into x
                    select new
                    {
                        month = prevMonth.Month,
                        year = prevMonth.Year,
                        completed = x.Where(i => i.appointment.status == "Completed").Count(),
                        cancelled = x.Where(i => i.appointment.status == "Pending" || i.appointment.status == "Scheduled")
                        .Count(),
                        nurseDuties =  x.Select(x => x.duty.nurse_assistant).Count(),
                        doctorDuties = x.Select(x => x.duty.doctor_id).Count(),
                    }).FirstOrDefaultAsync();


                //Count total of appointments
                var totalAppointments = await reportDbContext.p_appointments.
                    Where(i => i.appointment_date.Month == prevMonth.Month && 
                    i.appointment_date.Year == prevMonth.Year).CountAsync();

                var report = new month_appointment_and_duty_report()
                {
                    totalAppointments = totalAppointments,
                    year = prevMonth.Year,
                    month = prevMonth.Month,
                    completed = monthAppointmentsAndDutiesReport?.completed ?? 0,
                    cancelled = monthAppointmentsAndDutiesReport?.cancelled ?? 0,
                    pending = 0, 
                    doctorDuties = monthAppointmentsAndDutiesReport?.doctorDuties ?? 0, 
                    nurseDuties = monthAppointmentsAndDutiesReport?.nurseDuties ?? 0,                    
                };

                if (report != null)
                {
                    await reportDbContext.month_appointment_and_duty_report.AddAsync(report);
                }
                else
                {
                    _logger.LogInformation($"No transaction extracted for {prevMonth.Month} - {prevMonth.Year}");
                }

                await reportDbContext.SaveChangesAsync();
                _logger.LogInformation("Extraction finished!");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error : {ex.Message}");
            }
        }
    }
}

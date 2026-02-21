using System;
using APIResponses.forecast;
using APIResponses.Historical_report.Models;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.forecastService;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;
using Report_and_Analytics_API.Middleware;
using Report_and_Analytics_API.Repository;
using Report_and_Analytics_API.Service;
using Report_and_Analytics_API.service_helpers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;



Log.Logger = new LoggerConfiguration()
      .WriteTo.Console()
      .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
      .Enrich.FromLogContext()
      .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();


if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MainDb"),
        new MySqlServerVersion(new Version(8, 0, 36)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,             
            maxRetryDelay: TimeSpan.FromSeconds(10), 
            errorNumbersToAdd: null          
        )
    )
);


//builder.Services.AddHostedService<monthRevenueReportService>();
//builder.Services.AddHostedService<dailyAttendanceReportGeneratorService>();
//builder.Services.AddHostedService<monthShiftAndDutiesReport>();
//builder.Services.AddHostedService<monthLeaveServiceReport>();
//builder.Services.AddHostedService<monthPharmacySalesReport>();
//builder.Services.AddHostedService<departmentBudgetYearlyReportService>();
//builder.Services.AddHostedService<monthPayrollSummaryReportService>();
//builder.Services.AddHostedService<patientAdmissionAndDischargeSummaryReportService>();
//builder.Services.AddHostedService<monthAttendanceReportService>();
//builder.Services.AddHostedService<yearReportSummaryService>();
//builder.Services.AddHostedService<monthPerformanceReportService>();
//builder.Services.AddHostedService<monthBillingSummaryReportService>();
//builder.Services.AddHostedService<yearBillingSummaryReport>();
//builder.Services.AddHostedService<yearAdmissionAndBeddingSummary>();
//builder.Services.AddHostedService<yearPharmacySalesReport>();
//builder.Services.AddHostedService<monthBedOccupancyTrainingDataExtraction>();
//builder.Services.AddHostedService<patientAdmissionAndDischargeSummaryReportService>();
//builder.Services.AddHostedService<monthPatientAdmissionTrainingDataExtraction>();
//builder.Services.AddHostedService<monthOperationalCostReportService>();
//builder.Services.AddHostedService<monthCostManagementTrainingDataExtraction>();
//builder.Services.AddHostedService<monthMedicineSupplyPredictionTrainingDataExtraction>();
//builder.Services.AddHostedService<monthApprovedClaimAmountDataExtraction>();
//builder.Services.AddHostedService<monthNumberOfClaimsApprovedDataExtraction>();
//builder.Services.AddHostedService<monthBedOccupancyForecastingService>();
//builder.Services.AddHostedService<monthCostManagementForecastingService>();
//builder.Services.AddHostedService<MonthPatientAdmissionForecastingService>();
//builder.Services.AddHostedService<MonthRevenueForecastingService>();
//builder.Services.AddHostedService<monthProviderClaimStatusDataExtraction>();
//builder.Services.AddHostedService<MonthInsuranceClaimsStatusForecastingService>();
//builder.Services.AddHostedService<monthProviderClaimAmountHistoryDataExtraction>();
//builder.Services.AddHostedService<monthInsuranceClaimAmountForecastingService>();
//builder.Services.AddHostedService<monthStaffingNeedsDataExtraction>();
//builder.Services.AddHostedService<monthStaffingNeedsForecastingService>();
//builder.Services.AddHostedService<monthMedicineSupplyForecastingService>();
//builder.Services.AddHostedService<monthInsuranceClaimReportService>();
//builder.Services.AddHostedService<monthOutcomeReportDataExtractionService>();
//builder.Services.AddHostedService<yearlyClaimReportDataExtraction>();
//builder.Services.AddHostedService<yearPayrollBackgroundServiceDataExtraction>();
builder.Services.AddHostedService<dailyServiceUpdateYearPharmacySales>();
builder.Services.AddHostedService<dailyServiceUpdateYearBillingSummary>();
//builder.Services.AddHostedService<AttendanceBackgroundService>();
builder.Services.AddScoped<IhrLeaveRepository,hrLeaveRepository>();
builder.Services.AddScoped<IhrPayrollRepository,hrPayrollRepository>();
builder.Services.AddScoped<IhrEmployeeInformation,hrEmployeeInformation>();
builder.Services.AddScoped<IjournalRepository,journalRepository>();
builder.Services.AddScoped<IinsuranceClaimRepository, claimRepository>();
builder.Services.AddScoped<IpropertyRepository,propertyRepository>();
builder.Services.AddScoped<IemployeeRepository,employeeRepository>();
builder.Services.AddScoped<IjoblogsRepository,jobLogsRepository>();
builder.Services.AddScoped<IpatientAdmissionRepository,patientAdmissionRepository>();
builder.Services.AddSingleton<BedOccupancyPredictionService>();
builder.Services.AddSingleton<monthCostForecastService>();
builder.Services.AddSingleton<monthPatientAdmissionPredictionService>();
builder.Services.AddSingleton<monthRevenueForecastPredictionService>();
builder.Services.AddSingleton<monthInsuranceClaimsStatusPredictionService>();
builder.Services.AddSingleton<monthInsuranceClaimAmountPredictionService>();
builder.Services.AddSingleton<monthStaffingNeedsPredictionService>();
builder.Services.AddSingleton<monthMedicineShortagePredictionService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//CORS Configuration
app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyMethod();
    policy.AllowAnyHeader();
});

//app.UseMiddleware<apiKeyMiddleware>();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();


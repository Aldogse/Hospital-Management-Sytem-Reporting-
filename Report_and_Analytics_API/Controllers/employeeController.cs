using APIResponses.Employee_Responses;
using APIResponses.Historical_report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("employee/")]
    public class EmployeeController : ControllerBase
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ReportDbContext reportDbContext,ILogger<EmployeeController> logger)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
        }

        //ENDPOINT FOR DAILY ATTEDANCE REPORT
        [HttpGet("attendanceReport/{date}")]
        public async Task<IActionResult> attendanceReport(DateTime date)
        {
            try
            {
                var employees = await _reportDbContext.hr_employees.CountAsync();
                var report = await _reportDbContext.daily_attendance_report
                    .Where(i => i.reportDate == date)
                    .Select(i => new dailyAttendanceReportResponse
                    {
                        reportDate = i.reportDate.ToShortDateString(),
                        absent = i.absent,
                        late = i.late,
                        leave = i.leave,
                        present = i.present,
                        underTime = i.underTime,
                        totalEmployees = employees
                    }).FirstOrDefaultAsync();

                if (report == null)
                {
                    return Ok(new {});
                }
              
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("dates")]
        public async Task<IActionResult> dates()
        {
            try
            {
                var dates = await _reportDbContext.daily_attendance_report
                    .Select(i => i.reportDate).ToListAsync();

                if(dates == null)
                {
                    return Ok(new {});
                }

                return Ok(dates);
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //ENDPOINTS FOR SHIFT AND DUTY MONITORING
        [HttpGet("getMonthShiftAndDutyReport/{month}/{year}")]
        public async Task<IActionResult> getMonthShiftAndDutyReport(int month,int year)
        {
            try
            {
                    var report = await _reportDbContext.month_appointment_and_duty_report
                    .Where(i => i.year == year && i.month == month).FirstOrDefaultAsync();


                var appointments = await (
                    from appointment in _reportDbContext.p_appointments
                    join duty in _reportDbContext.duty_assignments
                    on appointment.appointment_id equals duty.appointment_id
                    where appointment.appointment_date.Month == month && 
                    appointment.appointment_date.Year == year 
                    group new { appointment , duty } by appointment.appointment_id into x
                    select new
                    {
                        Id = x.Key,
                        doctor = x.Select(d => d.duty.doctor_id).FirstOrDefault(),
                        bed = x.Select(b => b.duty.bed_id).FirstOrDefault(),
                        nurse = x.Select(n => n.duty.nurse_assistant).FirstOrDefault(),
                        procedure = x.Select(p => p.duty.procedure).FirstOrDefault(),
                        status = x.Select(s => s.duty.status).FirstOrDefault(),                      
                    }).ToListAsync();

                var response = new monthShiftAndDutyReportResponse()
                {
                    totalAppointments = report?.totalAppointments ?? 0,
                    completed = report?.completed ?? 0,
                    cancelled = report?.cancelled ?? 0,
                    doctorDuties = report?.doctorDuties ?? 0,
                    nurseDuties = report?.nurseDuties ?? 0,
                    appointments = appointments.Select(i => new duty_assignment
                    {
                        appointment_id = i.Id,
                        doctor_id = i.doctor,
                        bed_id = i.bed,
                        nurse_assistant = i.nurse,
                        procedure = i.procedure,
                        status = i.status,
               
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //ENDPOINT FOR MONTH EMPLOYEE PERFORMANCE AND EVALUATION REPORT
        [HttpGet("monthEvaluationAndPerformanceReport/{month}/{year}")]
        public async Task<IActionResult> monthEvaluationAndPerformanceReport(string month,int year)
        {
            try
            {
                var monthEmployeesPerformanceReport = await _reportDbContext.quarter_employees_performance_and_evaluation_report
                    .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();

               if(monthEmployeesPerformanceReport == null)
               {
                    return Ok(new
                    {
                        success = true,
                        message = $"No record was found for {month}/{year}",
                        data = (object?)null
                    });
               }

                var employeeDetails = await (
                    from evaluation in _reportDbContext.evaluation_summary_reports
                    join name in _reportDbContext.hr_employees
                    on evaluation.employee_id equals name.employee_id
                    where evaluation.evaluation_period == month 
                    group new { evaluation , name } by name.employee_id into x
                    select new evaluation_summary_reports
                    {
                        employee_id = x.Key,
                        average_score = x.Select(i => i.evaluation.average_score).FirstOrDefault(),
                        performance_level = x.Select(i => i.evaluation.performance_level).FirstOrDefault(),
                        evaluation_period = x.Select(i => i.evaluation.evaluation_period).FirstOrDefault(),
                        number_of_evaluations = x.Select(i => i.evaluation.number_of_evaluations).FirstOrDefault(),
                        last_evaluated = x.Select(i => i.evaluation.last_evaluated).FirstOrDefault()
                    }).ToListAsync();


                var response = new monthEmployeePerformanceAndEvaluationReportResponse()
                {
                    totalEmployeesEvaluated = monthEmployeesPerformanceReport.totalEmployeesEvaluated,
                    monthAveragePerformanceScore = monthEmployeesPerformanceReport.averagePerformanceScore,
                    monthNumberOfLowPerformers = monthEmployeesPerformanceReport.lowPerformers,
                    report = employeeDetails,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //ENDPOINT FOR DOCTOR EVALUATION AND DETAILS DASHBOARD
        [HttpGet("getDoctorsDetails")]
        public async Task <IActionResult> getDoctorsDetails()
        {
            try
            {
                var doctors = await _reportDbContext.hr_employees
                    .Where(i => i.profession == "Doctor" && i.status == "Active")
                    .Select(i => new
                    {
                        i.employee_id,
                        i.first_name,
                        i.last_name,
                        i.middle_name,
                        i.specialization,
                        i.role
                    }).ToListAsync();


                if(doctors == null || doctors.Count == 0)
                {
                    return Ok(new
                    {
                        succeed = true,
                        message = $"No doctors found",
                        data = (object?) null
                    });
                }

                return Ok(doctors);

            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getDoctorDetailsAndEvaluation/{doctorId}")]
        public async Task<IActionResult> getDoctorDetailsAndEvaluation(int doctorId)
        {
            try
            {
                var evaluations = await _reportDbContext.evaluation_records
                    .Where(i => i.employee_id == doctorId)
                    .Select(i => new
                    {
                        i.created_at,
                        i.score,
                        i.rating,
                        i.comments,
                    }).ToListAsync();

                if (evaluations?.Count > 0)
                {
                    var doctorDetails = await (
                        from docDetails in _reportDbContext.hr_employees
                        join evaluation in _reportDbContext.evaluation_records
                        on docDetails.employee_id equals evaluation.employee_id
                        where docDetails.employee_id == doctorId
                        group new { docDetails, evaluation } by 1 into x
                        select new doctorDetailsAndEvaluationSummaryResponse
                        {
                            department = x.Select(i => i.docDetails.department).FirstOrDefault() ?? "no records found",
                            specialization = x.Select(i => i.docDetails.specialization).FirstOrDefault() ?? "no records found",
                            role = x.Select(i => i.docDetails.role).FirstOrDefault() ?? "no records found",
                            employmentType = x.Select(i => i.docDetails.employment_type).FirstOrDefault() ?? "no records found",
                            degreeType = x.Select(i => i.docDetails.degree_type).FirstOrDefault() ?? "no records found",
                            educationalStatus = x.Select(i => i.docDetails.educational_status).FirstOrDefault() ?? "no records found",
                            graduationYear = x.Select(i => i.docDetails.graduation_year).FirstOrDefault() ?? 0,
                            licenseExpiry = x.Select(i => i.docDetails.license_expiry).FirstOrDefault(),
                            licenseIssued = x.Select(i => i.docDetails.license_issued).FirstOrDefault(),
                            licenseNumber = x.Select(i => i.docDetails.license_number).FirstOrDefault() ?? "no records found",
                            licenseType = x.Select(i => i.docDetails.license_type).FirstOrDefault() ?? "no records found",
                            medicalSchool = x.Select(i => i.docDetails.medical_school).FirstOrDefault() ?? "no records found",
                            evaluation_records = evaluations.Select(i => new doctorSummaryEvaluationResponse
                            {
                                created_at = i.created_at.ToShortDateString(),
                                score = i.score,
                                rating = i.rating,
                                comments = i.comments,
                            }).ToList()
                        }).FirstOrDefaultAsync();

                    return Ok(doctorDetails);
                }
                else
                {
                    var details = await (
                        from docDetails in _reportDbContext.hr_employees
                        where docDetails.employee_id == doctorId
                        group new { docDetails} by 1 into x
                        select new doctorDetailsAndEvaluationSummaryResponse
                        {
                            department = x.Select(i => i.docDetails.department).FirstOrDefault() ?? "no records found",
                            specialization = x.Select(i => i.docDetails.specialization).FirstOrDefault() ?? "no records found",
                            role = x.Select(i => i.docDetails.role).FirstOrDefault() ?? "no records found",
                            employmentType = x.Select(i => i.docDetails.employment_type).FirstOrDefault() ?? "no records found",
                            degreeType = x.Select(i => i.docDetails.degree_type).FirstOrDefault() ?? "no records found",
                            educationalStatus = x.Select(i => i.docDetails.educational_status).FirstOrDefault() ?? "no records found",
                            graduationYear = x.Select(i => i.docDetails.graduation_year).FirstOrDefault() ?? 0,
                            licenseExpiry = x.Select(i => i.docDetails.license_expiry).FirstOrDefault(),
                            licenseIssued = x.Select(i => i.docDetails.license_issued).FirstOrDefault(),
                            licenseNumber = x.Select(i => i.docDetails.license_number).FirstOrDefault() ?? "no records found",
                            licenseType = x.Select(i => i.docDetails.license_type).FirstOrDefault() ?? "no records found",
                            medicalSchool = x.Select(i => i.docDetails.medical_school).FirstOrDefault() ?? "no records found",
                            evaluation_records = new List<doctorSummaryEvaluationResponse>()
                        }).FirstOrDefaultAsync();

                    return Ok(details);
                }                
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }           
        }

        //END POINTS FOR STAFF REPORT - EMPLOYEE DIRECTORY
        [HttpGet("getStaffInformation")]
        public async Task<IActionResult> getStaffInformation()
        {
            try
            {
                var employees = await _reportDbContext.hr_employees.ToListAsync();

                var totalCount = employees.Count();
                var activeCount = employees.Where(i => i.status == "Active").Count();

                var employeeInformation =  employees.Select(i => new staffBasicInformation
                {
                    employeeId =  i.employee_id,
                    fullName = $"{i.first_name} {i.last_name}",
                    role = i.role ?? "No role found",
                    department = i.department ?? "",
                    specialization = i.specialization ?? "General Doctor",
                    employmentStatus = i.employment_type ?? "",
                    contact = i.contact_number ?? "No Contact number found.",
                    hireDate = i.hire_date,
                    status = i.status,
                }).ToList();


                var response =  employees.Select(i => new staffInformationReportResponse
                { 
                    totalEmployees = totalCount,
                    activeStaff =  activeCount,
                    employees = employeeInformation,
                }).FirstOrDefault();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
                return StatusCode(500,ex.Message);
            }
        }
    }
}

using APIResponses.Employee_Responses;
using APIResponses.Historical_report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("employee/")]
    public class EmployeeController : ControllerBase
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<EmployeeController> _logger;
        private readonly IemployeeRepository _empRepo;

        public EmployeeController(ReportDbContext reportDbContext,ILogger<EmployeeController> logger,IemployeeRepository empRepo)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
            _empRepo = empRepo;
        }

        //ENDPOINT FOR DAILY ATTEDANCE REPORT
        [HttpGet("attendanceReport/{date}")]
        public async Task<IActionResult> attendanceReport(DateTime date)
        {
            try
            {
                var employees = await _reportDbContext.hr_employees.CountAsync();
                var report = await _empRepo.getDayAttendanceReport(date);

                if (report == null)
                {
                    return Ok(new 
                    {
                        success = true,
                        message = $"No report for {date}",
                        data = (object?)null
                    });
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

        //ENDPOINT FOR MONTH ATTENDANCE REPORT
        [HttpGet("getMonthAttendanceReport/{month}/{year}")]
        public async Task<IActionResult> getMonthAttendanceReport(int month,int year) 
        {
            try
            {
                var report = await _empRepo.getMonthAttendanceReportSummary(month,year);

                
                if(report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data extracted {month}/{year}",
                        dates = (object?)null
                    });
                }

                var summary = new monthAttendanceReportResponse()
                {
                    late = report.late,
                    absent = report.absent,
                    leave = report.leave_count,
                    present = report.present,
                    underTime = report.underTime
                };
                             
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //ENDPOINT FOR YEARLY ATTENDANCE REPORT DASHBOARD
        [HttpGet("getYearAttendanceReport/{year}")]
        public async Task<IActionResult> getYearAttendanceReport(int year)
        {
            try
            {
                var report = await _empRepo.yearAttendanceSummary(year);

                if(report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data extracted for {year}",
                        data = (object?)null
                    });
                }
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message}");
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
            return Ok();
        }

        //ENDPOINT FOR DOCTOR EVALUATION AND DETAILS DASHBOARD
        [HttpGet("getDoctorsDetails")]
        public async Task <IActionResult> getDoctorsDetails([FromQuery]int page,[FromQuery]int size)
        {
            try
            {
                var doctors = await _empRepo.getDoctorsInformation(page,size);

                if(doctors == null || doctors.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No doctors currently active",
                        data = (object?)null
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

        [HttpGet("getYearAttendanceSummary")]
        public async Task<IActionResult> getYearAttendanceSummary([FromQuery]int year)
        {
            try
            {
                var response = await _empRepo.yearAttendanceSummary(year);

                if(response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data for {year}",
                        data = (object?)null
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //ENDPOINT FOR LEAVE REPORT 
        [HttpGet("getMonthLeaveReports/{month}/{year}")]
        public async Task<IActionResult> getMonthLeaveReports(int month, int year)
        {
            try
            {
                var prevMonthLeaveReports = await _reportDbContext.month_leave_report
                    .Where(i => i.month == month && i.year == year)
                    .FirstOrDefaultAsync();

                var leaveDetails = await _reportDbContext.hr_leave
                    .Where(i => i.submit_at.Month == month && i.submit_at.Year == year)
                    .ToListAsync();

                if (prevMonthLeaveReports == null)
                {
                    return Ok(new
                    {
                        successs = true,
                        message = $"No leave report for {month}/{year}",
                        data = (object?)null
                    });
                }

                var response = new monthLeaveReportResponse()
                {
                    total_leaves = prevMonthLeaveReports.total_leave_request,
                    approved = prevMonthLeaveReports.month_approved_leaves,
                    rejected = prevMonthLeaveReports.month_rejected_leaves,
                    pending = prevMonthLeaveReports.month_pending_leaves,
                    leaves = leaveDetails
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //ENDPOINT FOR PERFORMANCE REPORT
        [HttpGet("getEmployeeMonthReportPerformance")]
        public async Task<IActionResult> getEmployeeMonthReportPerformance([FromQuery]int month,[FromQuery]int year)
        {
            try
            {
                var response = await _empRepo.monthEmployeesPerformanceReport(month,year);

                if(response == null)
                {
                    return Ok(new
                    {
                        successs = true,
                        message = $"No record found for {month}/{year}",
                        data = (object?)null
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getMonthListReport/{month}/{year}/{page}/{size}")]
        public async Task<IActionResult> getMonthListReport(int month,int year,int page,int size)
        {
            try
            {
                var report = await _empRepo.getMonthEmployeePerformanceSummarryList(month,year,page,size);

                if(report == null || report.Count == 0)
                {
                    return Ok(new
                    {
                        successs = true,
                        message = $"No report for {month}/{year}",
                        data = (object?)null
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("monthAttendanceComparisonEndpoint")]
        public async Task<IActionResult> monthAttendanceComparisonEndpoint([FromQuery]int baseMonth,[FromQuery]int baseYear
            ,[FromQuery]int partnerMonth, [FromQuery]int partnerYear)
        {
            try
            {
                if (baseMonth == partnerMonth && baseYear == partnerYear)
                {
                    return StatusCode(400, "Same month and year not allowed");
                }
                var comparisonResponse = await _empRepo.monthAttendanceComparisonResponse(baseMonth, baseYear, partnerMonth, partnerYear);

                if (comparisonResponse == null)
                {
                    return Ok(new
                    {
                        success = "true",
                        message = "No report for the following month",
                        data = (object?)null
                    });
                }
                return Ok(comparisonResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500,$"Error:{ex.Message}");
            }
        }

        //FORECAST CONTROLLERS
        [HttpGet("getMonthStaffForecastNeeds")]
        public async Task<IActionResult> getMonthStaffForecastNeeds([FromQuery]int month, [FromQuery]int year)
        {
            try
            {
                var forecast = await _empRepo.getMonthStaffingForecastNeeds(month,year);

                if(forecast == null || forecast.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No forecast recorded for the month yet",
                    });
                }

                return Ok(forecast);
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                return StatusCode(500,ex.Message);
            }
        }
    }
}
  
using APIResponses.PatientResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("patient/")]
    public class patientController : ControllerBase
    {
        private readonly ILogger<patientController> _logger;
        private readonly ReportDbContext _reportDbContext;
        private readonly IpatientAdmissionRepository _repository;
        private readonly IConfiguration _configuration;

        public patientController(ILogger<patientController> logger, ReportDbContext reportDbContext, IpatientAdmissionRepository repository
            , IConfiguration configuration)
        {
            _logger = logger;
            _reportDbContext = reportDbContext;
            _repository = repository;
            _configuration = configuration;
        }


        //PAGINATED RESPONSE FOR PATIENT LISTS PAGE
        [HttpGet("patientDetails")]
        public async Task<IActionResult> patientDetails([FromQuery] int page, [FromQuery] int size)
        {
            try
            {
                var data = await _repository.patientInformation();
                var ages = await _repository.getAges();
                var patientsInfoResponse = data.Select(i => new patientInformationResponse
                {
                    age = i.age,
                    contact = i.contact,
                    fullName = i.fullName,
                    gender = i.gender,
                    patientId = i.patientId,
                })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToList();

                var response = new patientInformationOverviewResponse()
                {
                    averageAge = data.Average(i => i.age),
                    maleCount = data.Where(i => i.gender == "Male").Count(),
                    femaleCount = data.Where(i => i.gender == "Female").Count(),
                    patients = patientsInfoResponse,
                    ages = ages,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //ENDPOINT FOR PATIENT RECORDS PAGE
        [HttpGet("patientMedicalRecords/{patientId}")]
        public async Task<IActionResult> patientMedicalRecords(int patientId)
        {
            try
            {
                //GET ALL ASSOCIATED MEDICAL RECORDS
                var prevMed = await _reportDbContext.p_previous_medical_records
                    .Where(i => i.patient_id == patientId)
                    .Select(i => new prevMedicalRecordsResponse
                    {
                        recordId = i.record_id,
                        conditionName = i.condition_name ?? "",
                        diagnosisDate = i.diagnosis_date,
                        notes = i.notes,
                    })
                    .OrderBy(i => i.diagnosisDate)
                    .ToListAsync();


                //QUERY FOR EXTRACTING ALL THE NEEDED DATA`
                var patientMedicalRecords = await (
                    from patient in _reportDbContext.patientinfo
                    join medrecs in _reportDbContext.p_previous_medical_records
                    on patient.patient_id equals medrecs.patient_id
                    where patient.patient_id == patientId
                    group new { patient, medrecs } by medrecs.record_id into x
                    select new patientMedicalRecordReport
                    {
                        patientId = x.Select(x => x.patient.patient_id).FirstOrDefault(),
                        address = x.Select(x => x.patient.address).FirstOrDefault() ?? "",
                        fullName = $"{x.Select(x => x.patient.fname).FirstOrDefault()} {x.Select(x => x.patient.mname).FirstOrDefault()} " +
                        $"{x.Select(x => x.patient.lname).FirstOrDefault()}",
                        gender = x.Select(x => x.patient.gender).FirstOrDefault() ?? "",
                        prevMedRecs = prevMed
                    }).FirstOrDefaultAsync();

                if (patientMedicalRecords == null)
                {
                    return Ok(new
                    {
                        sucess = true,
                        message = $"No record found for patient ID : {patientId}",
                        data = (object?)null
                    });
                }

                return Ok(patientMedicalRecords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        //FORECAST ENDPOINTS
        [HttpGet("getPatientAdmissionForecast")]
        public async Task<IActionResult> getPatientAdmissionForecast()
        {
            try
            {
                var date = DateTime.UtcNow;
                var report = await _repository.getMonthPatientForecast(date.Month, date.Year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No forecast yet for the month"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getPreviousMonthsAdmissionReport")]
        public async Task<IActionResult> getPreviousMonthsAdmissionReport()
        {
            try
            {
                var prevMonth = DateTime.UtcNow.AddMonths(-1);
                var report = await _repository.getPreviousMonthsPatientAdmission(prevMonth.Year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No data"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("GetAdvancedAnalytics")]
        public async Task<IActionResult> GetAdvancedAnalytics(
 [FromQuery] DateTime startDate,
 [FromQuery] DateTime endDate)
        {
            // STEP 1: Load raw rows from DB (NO GROUPING HERE)
            var rawData = await (
                from a in _reportDbContext.p_bed_assignments
                join p in _reportDbContext.patientinfo on a.patient_id equals p.patient_id
                join med in _reportDbContext.p_previous_medical_records on a.patient_id equals med.patient_id into medical
                from med in medical.DefaultIfEmpty()
                where
                    (a.assigned_date >= startDate && a.assigned_date <= endDate) ||
                    (a.released_date != null && a.released_date >= startDate && a.released_date <= endDate)
                select new
                {
                    a.assignment_id,
                    a.patient_id,
                    p.fname,
                    p.lname,
                    p.age,
                    p.gender,
                    a.bed_id,
                    a.assigned_date,
                    a.released_date,
                    condition_name = med.condition_name
                }
            ).ToListAsync();

            // STEP 2: Remove duplicate assignments IN MEMORY (MySQL-safe)
            var raw = rawData
                .GroupBy(x => x.assignment_id)
                .Select(g => g.First())
                .ToList();

            if (!raw.Any())
                return Ok(new { success = true, message = "No data found" });

            // STATISTICS
            var now = DateTime.Today;
            var losList = raw.Select(x =>
            {
                DateTime start = x.assigned_date;
                DateTime end = x.released_date ?? now;
                return (end - start).TotalDays;
            }).ToList();

            return Ok(new
            {
                totalPatients = raw.Select(r => r.patient_id).Distinct().Count(),
                activePatients = raw.Count(r => r.released_date == null),
                dischargedPatients = raw.Count(r => r.released_date != null),

                male = raw.Count(x => x.gender == "Male"),
                female = raw.Count(x => x.gender == "Female"),

                avgAge = raw.Average(x => x.age),
                minAge = raw.Min(x => x.age),
                maxAge = raw.Max(x => x.age),

                ageDistribution = raw
                    .GroupBy(x => x.age)
                    .Select(g => new { age = g.Key, count = g.Count() })
                    .OrderBy(x => x.age),

                conditions = raw
                    .Where(x => !string.IsNullOrEmpty(x.condition_name))
                    .GroupBy(x => x.condition_name)
                    .Select(g => new { condition = g.Key, count = g.Count() }),

                losAverage = losList.Average(),
                losDistribution = losList,

                bedUsage = raw
                    .GroupBy(x => x.bed_id)
                    .Select(g => new { bed = g.Key, count = g.Count() }),

                admissionsPerMonth = raw
                    .GroupBy(x => new { x.assigned_date.Year, x.assigned_date.Month })
                    .Select(g => new { month = $"{g.Key.Year}-{g.Key.Month:D2}", count = g.Count() })
                    .OrderBy(x => x.month),

                patientDetails = raw
            });
        }



        [HttpGet("getCensus")]
        public async Task<IActionResult> GetCensus(
           [FromQuery] DateTime? startDate,
           [FromQuery] DateTime? endDate)
        {
            try
            {
                if (startDate == null || endDate == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "startDate and endDate are required (YYYY-MM-DD)"
                    });
                }

                // STEP 1: Load data from MySQL with NO GROUPING
                var rawData = await (
                     from a in _reportDbContext.p_bed_assignments
                     join p in _reportDbContext.patientinfo on a.patient_id equals p.patient_id
                     join med in _reportDbContext.p_previous_medical_records on a.patient_id equals med.patient_id into medical
                     from med in medical.DefaultIfEmpty()
                     where
                         (a.assigned_date >= startDate && a.assigned_date <= endDate) ||
                         (a.released_date != null && a.released_date >= startDate && a.released_date <= endDate)
                     select new
                     {
                         a.assignment_id,
                         a.patient_id,
                         p.fname,
                         p.lname,
                         p.age,
                         p.gender,
                         a.bed_id,
                         a.assigned_date,
                         a.released_date,
                         condition_name = med.condition_name
                     }
                 ).ToListAsync();

                // STEP 2: Remove duplicates IN MEMORY
                var result = rawData
                    .GroupBy(x => x.assignment_id)
                    .Select(g => g.First())
                    .ToList();

                if (!result.Any())
                {
                    return Ok(new { success = true, message = "No data found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        //LABORATORY

        [HttpGet("lab_ct")]
        public async Task<IActionResult> lab_ct()
        {
            try
            {
                var response = await _reportDbContext.dl_lab_ct.ToListAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }


        [HttpGet("lab_mri")]
        public async Task<IActionResult> lab_mri()
        {
            try
            {
                var response = await _reportDbContext.dl_lab_mri.ToListAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("lab_tools_used")]
        public async Task<IActionResult> lab_tools_used()
        {
            try
            {
                var response = await _reportDbContext.dl_lab_tools_used.ToListAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("lab_xray")]
        public async Task<IActionResult> lab_xray()
        {
            try
            {
                var response = await _reportDbContext.dl_lab_xray.ToListAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("lab_results")]
        public async Task<IActionResult> lab_results()
        {
            try
            {
                var response = await _reportDbContext.dl_results.ToListAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}


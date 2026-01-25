using APIResponses.PatientResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public patientController(ILogger<patientController> logger, ReportDbContext reportDbContext,IpatientAdmissionRepository repository)
        {
            _logger = logger;
            _reportDbContext = reportDbContext;
            _repository = repository;
        }


        //PAGINATED RESPONSE FOR PATIENT LISTS PAGE
        [HttpGet("patientDetails/{page}/{size}")]
        public async Task<IActionResult> patientDetails(int page, int size)
        {
            try
            {
                var response = await _reportDbContext.patientinfo
                    .OrderBy(i => i.patient_id)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Select(i => new patientInformationResponse
                    {
                        patientId = i.patient_id,
                        fullName = $"{i.fname} {i.mname} {i.lname}",
                        age = i.age,
                        contact = i.phone_number,
                        gender = i.gender
                    }).ToListAsync();

                if(response == null || response.Count <= 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data was extracted",
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

        //ENDPOINT FOR PATIENT RECORDS PAGE
        [HttpGet("patientMedicalRecords/{patientId}")]
        public async Task<IActionResult> patientMedicalRecords(int patientId)
        {
            try
            {
                //GET ALL ASSOCIATED MEDICAL RECORDS
                var prevMed = await _reportDbContext.p_previous_medical_records
                    .Where( i => i.patient_id == patientId)
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
                    group new { patient , medrecs } by medrecs.record_id into x
                    select new patientMedicalRecordReport
                    {
                        patientId = x.Select(x => x.patient.patient_id).FirstOrDefault(),
                        address = x.Select(x => x.patient.address).FirstOrDefault() ?? "",
                        fullName = $"{x.Select(x => x.patient.fname).FirstOrDefault()} {x.Select(x => x.patient.mname).FirstOrDefault()} " +
                        $"{x.Select(x => x.patient.lname).FirstOrDefault()}",
                        gender = x.Select(x => x.patient.gender).FirstOrDefault() ?? "",
                        prevMedRecs = prevMed                     
                   }).FirstOrDefaultAsync();

                if(patientMedicalRecords == null)
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
                return StatusCode(500,ex.Message);
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
    }
}

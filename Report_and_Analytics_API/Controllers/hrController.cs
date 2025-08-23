using APIResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("Hr/")]
    public class hrController : ControllerBase
    {
        private readonly IhrPayrollRepository _payrollRepository;
        private readonly IhrEmployeeInformation _employeeInformation;
        private readonly ReportDbContext _reportDbContext;

        public hrController(IhrPayrollRepository payrollRepository,IhrEmployeeInformation employeeInformation,
            ReportDbContext reportDbContext)
        {
         
            _payrollRepository = payrollRepository;
            _employeeInformation = employeeInformation;
            _reportDbContext = reportDbContext;
        }

        //CURRENT DEDUCTIONS
        [HttpGet("getPayrollInformation/{employeeId}")]
        public async Task<IActionResult> GetCurrentDeductions(int employeeId)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                return Ok();
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }


        //YEAR TO DATE DEDUCTIONS
        [HttpGet("getYearToDatePayrollInformation/{employeeId}")]
        public async Task<IActionResult> GetYTDDeductions(int employeeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var payrolls = await _reportDbContext.hr_payroll.Where(i => i.employee_id == employeeId).ToListAsync();
                var deductions = new ytdDeductions();

                foreach(var item in payrolls)
                {
                    deductions.ytdGrossPay += item.gross_pay;
                    deductions.netPay += item.net_pay;
                    deductions.deductions += item.total_deductions;
                    deductions.absenceDeduction += item.absence_deduction;
                    deductions.loanDeduction += item.loan_deduction;
                    deductions.sssDeduction += item.sss_deduction;
                    deductions.philHealthDeduction += item.philhealth_deduction;
                }

                return Ok(deductions);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}

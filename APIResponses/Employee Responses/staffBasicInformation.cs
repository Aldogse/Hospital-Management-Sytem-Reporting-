using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Employee_Responses
{
    public class staffBasicInformation
    {
        public int employeeId { get; set; }
        public string fullName { get; set; }
        public string role { get; set; }
        public string department { get; set; }
        public string specialization { get; set; }
        public string employmentStatus { get; set; }
        public string contact { get; set; }
        public DateOnly? hireDate { get; set; }
        public string status { get; set; }
    }
}

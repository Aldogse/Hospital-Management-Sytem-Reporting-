using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Laboratory
{
    public class dl_lab_mri
    {
        [Key]
        public int id { get; set; }
        public int scheduleID { get; set; }
        public int patientID { get; set; }
        public string? testType { get; set; }
        public string? findings { get; set; }
        public string? impression { get; set; }
        public string? remarks { get; set; }
        public DateTime created_at { get; set; }
        public int processed_by { get; set; }
    }
}

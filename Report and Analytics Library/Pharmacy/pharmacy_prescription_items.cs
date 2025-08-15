using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Pharmacy
{
    public class pharmacy_prescription_items
    {
        [Key]
        public int item_id { get; set; }


        [ForeignKey("pharmacy_Prescription")]
        public int? prescription_id { get; set; }
        [JsonIgnore]
        public pharmacy_prescription pharmacy_Prescription { get; set; }


        [ForeignKey("pharmacy_Inventory")]
        public int? med_id { get; set; }
        [JsonIgnore]
        public pharmacy_inventory pharmacy_Inventory { get; set; }


        public string? dosage { get; set; }
        public int? quantity_prescribed { get; set; }
        public int? quantity_dispensed { get; set; }
    }
}

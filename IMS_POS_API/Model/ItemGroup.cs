using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Model
{
    public class ItemGroup
    {
        [Required]
        [StringLength(20)]
        public string GroupCode { get; set; }
        [Required]
        [StringLength(20)]
        public string Name { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Status { get; set; }
        public string Signature { get; set; }
       // public string Hash { get; set; }
    }

    public class Item
    {
        [Required]
        [StringLength(20)]
        public string SkuCode { get; set; }
        public string BarCode { get; set; }
        [Required]
        [StringLength(20)]
        public string Name { get; set; }
        [Required]
        //[StringLength(20)]
        public string UOM { get; set; }//UOM Unit Of Measurement
        public string AlternateUOM { get; set; }
        public float AlternateQuantity { get; set; }
        public float AlternateMRP { get; set; }
        [Required]
        [StringLength(20)]
        public string GroupCode { get; set; }
        [Required]
        [StringLength(20)]
        public string ItemType { get; set; }
        [Required]
        //[StringLength(20)]
        public float MRP { get; set; }
        [Required]
        //[StringLength(20)]
        public bool IsVatItem { get; set; }
        public int Status { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Signature { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Model
{
    public class TBtoBSales
    {
        [Required]
        [StringLength(20)]
        public string RefInvoiceNo { get; set; }
        public String TranDate { get; set; }
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }
        [Required]
        [StringLength(20)]
        public string CustomerPan { get; set; }
        [Required]
        [StringLength(100)]
        public string Customeraddress { get; set; }
        [Required]
        [StringLength(25)]
        public string PaymentMode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
        [Required]
        public string Signature { get; set; }
        public TInvoiceDetail[] ItemList { get; set; }
    }
    public class BtoBSales : TBtoBSales
    {
        public string ADDRESS { get; set; }
        public string ACNAME { get; set; }
        public string ACID { get; set; }
        public string CurNo { get; set; }
        public string PhiscalID { get; set; }
        public string DIV { get; set; }
        public string PARENT { get; set; }
        public string DIVISION { get; set; }
        public string VCHRNO { get; set; }
        public string VNUM { get; set; }
        public string VNAME { get; set; }
        public string VoucherType { get; set; }
        public decimal AMOUNT { get; set; }
        public decimal DISAMOUNT { get; set; }
        public decimal VATAMOUNT { get; set; }
        public decimal NETAMOUNT { get; set; }
        public int TOTQTY { get; set; }
        public decimal DISCOUNT { get; set; }
        public string BSDATE { get; set; }
        
        public string LOGINUSER { get; set; }
        public string ENTRYTIME { get; set; }
        public string FDRATE { get; set; }
        public string TRNDATE { get; set; }
        public string TRNTIME { get; set; }
        public string TRNUSER { get; set; }
        public string ORDERNO { get; set; }
        public string TRNAC { get; set; }
        public string BILLTO { get; set; }
        public string BILLTOADD { get; set; }
        public string BILLTOTELL { get; set; }
        public float STAMP { get; set; }
        public string MCODE { get; set; }
        public string SNO { get; set; }
        public string UNIT { get; set; }
        public  string REFOPDID { get; set; }
        public new List<InvoiceDetail> ItemList { get; set; }
        public BtoBSales(TBtoBSales p)
        {
            this.RefInvoiceNo = p.RefInvoiceNo;
            this.TranDate = p.TranDate;
            this.CustomerName = p.CustomerName;
            this.CustomerPan = p.CustomerPan;
            this.Customeraddress = p.Customeraddress;
            this.PaymentMode = p.PaymentMode;
            this.InvoiceType = p.InvoiceType;
            this.ItemList = new List<InvoiceDetail>();

        }
    }
    public class TInvoiceDetail
    {
        [Required]
        [StringLength(20)]
        public string SkuCode { get; set; }
        public string UOM { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        //[StringLength(20)]
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
    }
    public class InvoiceDetail:TInvoiceDetail
    {
        public string MCODE { get; set; }
        public byte VAT { get; set; }
        public string DIVISION { get; set; }
        public string VCHRNO { get; set; }
        public string PhiscalID { get; set; }
        public decimal Amount { get; set; }
        //public decimal Discount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string REFOPDID { get; set; }
        public int SNO { get; set; }
        public InvoiceDetail(TInvoiceDetail s)
        {
            this.SkuCode = s.SkuCode;
            this.UOM = s.UOM;
            this.Quantity = s.Quantity;
            this.Rate = s.Rate;
            this.Discount = s.Discount;
        }
    }
    public class SalesInfo
    {
        public string VCHRNO { get; set; }
        public string REFBILL { get; set; }
        public byte STATUS { get; set; }
    }
    public class CustomerInfo
    {
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string MyProperty { get; set; }
    }

}

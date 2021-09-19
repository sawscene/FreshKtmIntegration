using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Model
{
    public class TBtoCSales
    {
        [Required]
        public string InvoiceNo { get; set; }
        
        public string RefInvoiceNo { get; set; }
        [Required]
        public string InvoiceDate { get; set; }
        [Required]
        public string InvoiceMiti { get; set; }
        public string Remarks { get; set; }
        [Required]
        public string OutletCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPan { get; set; }
        [Required]
        public string PaymentMode { get; set; }
        [Required]
        public string InvoiceType { get; set; }
        [Required]
        public int TotalAmount { get; set; }
        [Required]
        public int TotalDiscount { get; set; }
        [Required]
        public int TotalTaxable { get; set; }
        [Required]
        public int TotalNonTaxable { get; set; }
        [Required]
        public int TotalVat { get; set; }
        [Required]
        public int GrossBillAmount { get; set; }
        [Required]
        public TPaymentDetail[] PaymentDetail { get; set; }
        [Required]
        public TItemDetail[] ItemDetail { get; set; }    
        
    }
    public class TPaymentDetail
    {
        public string PaymentMode { get; set; }
        public int Amount { get; set; }
    }
    public class TItemDetail 
    {
        public string SkuCode { get; set; }
        public string UOM { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Taxable { get; set; }
        public decimal NonTaxable { get; set; }
        public decimal VAT { get; set; }
        public decimal GrossAmount { get; set; }
        public string SkuName { get; set; }
    }
    public class PaymentDetail:TPaymentDetail
    {
        public PaymentDetail(TPaymentDetail p)
        {
            this.PaymentMode = p.PaymentMode;
            this.Amount = p.Amount;
        }
    }
    public class ItemDetail:TItemDetail
    {
        public ItemDetail(TItemDetail p)
        {
            this.SkuCode = p.SkuCode;
            this.UOM = p.UOM;
            this.Quantity = p.Quantity;
            this.Rate = p.Rate;
            this.Amount = p.Amount;
            this.Taxable = p.Taxable;
            this.NonTaxable = p.NonTaxable;
            this.VAT = p.VAT;
            this.GrossAmount = p.GrossAmount;
            this.SkuName = p.SkuName;
        }
    }
    public class BtoCSales:TBtoCSales
    {
        public new List<PaymentDetail> PaymentDetail { get; set; }
        public new List<ItemDetail> ItemDetail { get; set; }
        public BtoCSales(TBtoCSales p)
        {
            this.InvoiceNo = p.InvoiceNo;
            this.RefInvoiceNo = p.RefInvoiceNo;
            this.InvoiceDate = p.InvoiceDate;
            this.InvoiceMiti = p.InvoiceMiti;
            this.Remarks = p.Remarks;
            this.OutletCode = p.Remarks;
            this.CustomerName = p.CustomerName;
            this.CustomerAddress = p.CustomerAddress;
            this.CustomerPan = p.CustomerPan;
            this.PaymentMode = p.PaymentMode;
            this.InvoiceType = p.InvoiceType;
            this.TotalAmount = p.TotalAmount;
            this.TotalDiscount = p.TotalDiscount;
            this.TotalTaxable = p.TotalTaxable;
            this.TotalNonTaxable = p.TotalNonTaxable;
            this.TotalVat = p.TotalVat;
            this.GrossBillAmount = p.GrossBillAmount;
            
            this.PaymentDetail = new List<PaymentDetail>();
            this.ItemDetail = new List<ItemDetail>();
        }
    }

}

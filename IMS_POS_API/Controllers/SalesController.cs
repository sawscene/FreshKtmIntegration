
using IMS_POS_API.Helper;
using IMS_POS_API.Model;
using IMS_POS_API.Models;
using IMS_POS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class SalesController : ControllerBase
    {
        private readonly SalesService salesService;
        public SalesController(SalesService _salesService)
        {
            this.salesService = _salesService;
        }
        [HttpPost]
       // [Authorize]
        [Route("api/SaveSales")]
        public async Task<IActionResult> SaveSale([FromBody] TBtoBSales sales)
        {
            try
            {
                decimal TotalAmount=0;
                var Key = ConnectionModel.KEY;
                var itemlistCount = sales.ItemList.Count();
                foreach(TInvoiceDetail amount in sales.ItemList )
                {
                    decimal Amount = amount.Rate * amount.Quantity;
                    TotalAmount += Amount;
                }
                //var msg = $@"{sales.TimeStamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")}-{sales.RefInvoiceNo}-{sales.TranDate}-{sales.CustomerPan}-{sales.PaymentMode}-{sales.InvoiceType}-{itemlistCount}-{TotalAmount}-{Key}";
                //string signature = HelperClass.HMAC_SHA256(Key, msg);
                //if (HelperClass.CheckHash(Key, msg, sales.Signature) == false)
                //{
                //    throw new Exception("Invalid data values");
                //}
                BtoBSales s = new BtoBSales(sales);
                foreach(TInvoiceDetail detail in sales.ItemList)
                {
                    s.ItemList.Add(new InvoiceDetail(detail));
                }
                dynamic response= await salesService.InsertSale(s);
                if (response.status == "ok")
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception Ex) 
            {
                return new BadRequestObjectResult(new FunctionResponse { status = "error", result = Ex.GetBaseException().Message });
            }
        }
        [HttpPost]
        [Authorize]
        [Route("api/Save")]
        
        public async Task<IActionResult> Save([FromBody] TBtoCSales sales)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new FunctionResponse { status = "error", result = ModelState });
                }
                //BtoCSales s = new BtoBSales(sales);
                //foreach (TInvoiceDetail detail in sales.ItemList)
                //{
                //    s.ItemList.Add(new InvoiceDetail(detail));
                //}
                //dynamic response = await salesService.InsertSale(s);
                //if (response.status == "ok")
                //{
                //    return Ok(response);
                //}
                //else
                //{
                //    return BadRequest(response);
                //}
            }
            catch (Exception Ex)
            {

                throw Ex;
            }
            return null;
        }
    }
}

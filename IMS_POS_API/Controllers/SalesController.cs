
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
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly SalesService salesService;
        public SalesController(SalesService _salesService)
        {
            this.salesService = _salesService;
        }
        [HttpPost]
        [Route("SaveSales")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveSale([FromBody] TBtoBSales sales)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(new FunctionResponse { status = "error", result = ModelState });
                }
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

                throw Ex;
            }
        }
        [HttpPost]
        [Route("Save")]
        [AllowAnonymous]
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

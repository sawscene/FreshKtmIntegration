//using ClientApi.Controllers;
using IMS_POS_API.DAL;
using IMS_POS_API.Helper;
using IMS_POS_API.Model;
using IMS_POS_API.Models;
using IMS_POS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]

    public class ItemController : ControllerBase
    {
        private readonly ItemSaveDataAccess itemSaveDataAccess;
        public ItemController(ItemSaveDataAccess _itemSaveDataAccess)
        {
            this.itemSaveDataAccess = _itemSaveDataAccess;
        }

        
        [HttpPost]
        [Route("api/saveItemGroup")]
        [AllowAnonymous]
        public async Task<ActionResult> SaveItemGroup([FromBody] List<ItemGroup> param)
        {
            try
            {
                //var connectinUser = User.Identity.Name;
                //var conString = ConnectionModel.ConnectionString;
                var Key = ConnectionModel.KEY;
                //DateTime TimeStamp = System.DateTime.Now;
                IList<ItemGroup> ItemGroupList = param;
                foreach(ItemGroup grp in ItemGroupList)
                {
                    var msg = $@"{grp.TimeStamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")}-{grp.GroupCode}-{grp.Name}-{Key}";
                    string signature = HelperClass.HMAC_SHA256(Key, msg);
                    if (HelperClass.CheckHash(Key, msg, grp.Signature) == false)
                    {
                        throw new Exception("Invalid data values");
                    }
                    dynamic result = await itemSaveDataAccess.SaveGroup(param);
                    if (result.status == "ok")
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest(result);
                    }
                }
                return Ok();
            }
            catch (Exception Ex)
            {

                throw Ex;
            }
        }
        [HttpPost]
        [Route("api/saveItem")]
        [AllowAnonymous]
        public async Task<ActionResult> SaveItem([FromBody] List<Item> param)
        {
            try
            {
                var Key = ConnectionModel.KEY;
                IList<Item> ItemList = param;
                foreach(Item grp in ItemList)
                {
                    var msg = $@"{grp.TimeStamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")}-{grp.SkuCode}-{grp.BarCode}-{grp.Name}-{grp.UOM}-{grp.MRP}-{Key}";
                    var signature = HelperClass.HMAC_SHA256(Key, msg);
                    if (HelperClass.CheckHash(Key, msg, grp.Signature) == false)
                    {
                        throw new Exception("Invalid data values");
                    }
                    dynamic result = await itemSaveDataAccess.SaveItem(param);
                    if (result.status == "ok")
                    {
                        return Ok(result);
                    }
                }
                return Ok();
                //dynamic result = await itemSaveDataAccess.SaveItem(param);
                //if (result.status == "ok")
                //{
                //    return Ok(result);
                //}
                //else
                //{
                //    return BadRequest(result);
                //}

            }
            catch (Exception)
            {

                throw;
            }
        }
       

    }
}
    























using Dapper;
using IMS_POS_API.Model;
using IMS_POS_API.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Services
{
    public class SalesService
    {
        string db;
            public SalesService(IConfiguration config)
        {
            db = config.GetConnectionString("myConnectionString");
        }

        public async Task<dynamic> InsertSale(BtoBSales s)
        {
            List<ErrorList> elist=new List<ErrorList>();
            try
            {
                
                using (SqlConnection conn = new SqlConnection(db))
                {
                    //DateTime trndate = conn.ExecuteScalar<DateTime>("select getdate()");
                    //TimeSpan date = trndate - s.TimeStamp;
                    //if (trndate.Subtract(s.TimeStamp).TotalMinutes > 5)
                    //{

                    //    throw new Exception("time limit is not valid");
                    //}
                    var SalesInfo = conn.QueryFirstOrDefault<SalesInfo>("SELECT VCHRNO, REFNO, ISNULL(STATUS, 0) STATUS FROM RMD_ORDERMAIN WHERE ORDERNO = @RefInvoiceNo",new { RefInvoiceNo = s.RefInvoiceNo });
                    if (SalesInfo is { })
                    {
                        elist.Add(new ErrorList { FieldName = "REFNO", ErrorMessage = $"REFNO: '{s.RefInvoiceNo}' is already saved & Bill is already processed."});
                        return new { status = "error", errorList = elist };
                    }
                    s.ORDERNO = s.RefInvoiceNo;
                    foreach (InvoiceDetail prod in s.ItemList)
                    {
                        prod.MCODE = await conn.ExecuteScalarAsync<string>($"SELECT MCODE FROM MENUITEM WHERE MCODE = @SkuCode", new { prod.SkuCode});
                        if (string.IsNullOrEmpty(prod.MCODE))
                            elist.Add(new ErrorList { FieldName = "ItemList[].ItemCode", ErrorMessage = $"Item Code '{prod.SkuCode}' does not exists." });
                        else
                        {

                            prod.VAT = await conn.ExecuteScalarAsync<byte>($"SELECT VAT FROM MENUITEM WHERE MENUCODE = @SkuCode", new { prod.SkuCode });
                            if (string.IsNullOrEmpty(prod.UOM))
                                prod.UOM = await conn.ExecuteScalarAsync<string>($"SELECT BASEUNIT FROM MENUITEM WHERE MCODE = @SkuCode", new { prod.SkuCode });
                        }
                    }
                    if (elist.Count > 0)
                        return new { errorlist = elist };
                    var division = (await conn.QueryAsync("SELECT INITIAL, PhiscalID FROM COMPANY;")).FirstOrDefault();
                    s.ACNAME = s.CustomerName;
                    s.ADDRESS = s.Customeraddress;
                    s.VNUM = s.CustomerPan;
                    s.DIVISION = division.INITIAL;
                    s.PhiscalID = division.PhiscalID;
                    s.TRNTIME=DateTime.Now.ToString("hh:mm tt");
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        if(s.PaymentMode=="Cash")
                        {
                            s.ACID = "AT01002";
                        }
                        else
                        {
                            s.ACID = await conn.ExecuteScalarAsync<string>($"select acid from rmd_aclist where VATNO =@CustomerPan",new { s.CustomerPan}, transaction: tran);
                            if (string.IsNullOrEmpty(s.ACID))
                            {
                                string acliststr = @"INSERT INTO RMD_ACLIST(ACID, ACNAME, PARENT, TYPE,  IsBasicAc, ADDRESS, VATNO, PType)
                                                 VALUES(@ACID, @ACNAME,'PAG2187','A',0,@ADDRESS,@VNUM,'C')";

                                s.ACID = "PA" + (await GETNEWSEQUENCES("Salesorder", tran)) + "X";
                                await conn.ExecuteAsync(acliststr, s, transaction: tran);
                                //await sqlcon.ExecuteAsync("update RMD_SEQUENCES set CurNo=CurNo+1  where VNAME='PARTYAC';", transaction: tran);
                            }
                        }
                        if (string.IsNullOrEmpty(s.VCHRNO))
                        {
                            s.VCHRNO = "SO" + (await GETNEWSEQUENCES("Salesorder", tran, s.DIVISION, s.PhiscalID))+"-"+s.DIVISION+"-"+s.PhiscalID;//SALESORDER
                        }
                        else
                        {
                            await conn.ExecuteAsync("DELETE FROM RMD_ORDERPROD WHERE VCHRNO = @VCHRNO AND DIVISION = @DIVISION AND PhiscalID = @PhiscalID", new { s.VCHRNO, s.DIVISION, s.PhiscalID }, tran);
                            await conn.ExecuteAsync("DELETE FROM RMD_ORDERMAIN WHERE VCHRNO = @VCHRNO AND DIVISION = @DIVISION AND PhiscalID = @PhiscalID", new { s.VCHRNO, s.DIVISION, s.PhiscalID }, tran);
                        }
                        s.BSDATE = await conn.ExecuteScalarAsync<string>($"select miti from DATEMITI where ad= @date;", new { date = Convert.ToDateTime(s.TranDate) }, transaction: tran);
                        s.ENTRYTIME = DateTime.Now.ToString("hh:mm tt");
                        if (s.DISAMOUNT > 0)
                            s.FDRATE = s.DISAMOUNT.ToString("#.##") + "%";
                        int sno = 0;
                        foreach (InvoiceDetail prod in s.ItemList)
                        {
                            
                            prod.VCHRNO = s.VCHRNO;
                            prod.DIVISION = s.DIVISION;
                            prod.PhiscalID = s.PhiscalID;
                            prod.Amount = prod.Quantity * prod.Rate;
                            prod.Discount = prod.Amount * s.DISAMOUNT / 100;
                            prod.VatAmount = (((prod.Amount - prod.Discount) / 1.13m) * 1.13m) / 100;
                            prod.NetAmount = prod.Amount - prod.Discount;
                            prod.SNO = ++sno;
                        }
                        s.TRNDATE = s.TranDate;
                        s.AMOUNT = s.ItemList.Sum(x => x.Amount);
                        s.DISCOUNT = s.ItemList.Sum(x => x.Discount);
                        s.VATAMOUNT = s.ItemList.Sum(x => x.VatAmount);
                        s.NETAMOUNT = s.ItemList.Sum(x => x.NetAmount);
                        s.TOTQTY = Convert.ToInt32(s.ItemList.Sum(x => x.Quantity));


                        string strOrderMain = @"INSERT INTO RMD_ORDERMAIN(VCHRNO, DIVISION, PhiscalID, TRNDATE, BSDATE, TRNTIME,  TRNUSER, ORDERNO, TRNAC, STAMP)
                                                                VALUES (@VCHRNO, @DIVISION, DBO.GETPHISCALID(), @TRNDATE, @BSDATE, @TRNTIME, @TRNUSER, @ORDERNO, @TRNAC, CONVERT(FLOAT, GETDATE()))";
                        string strOrderProd = @"INSERT INTO RMD_ORDERPROD(VCHRNO, DIVISION, PhiscalID, UNIT, QUANTITY, RATE, AMOUNT, MCODE, SNO, STAMP)
                                                                    VALUES (@VCHRNO, @DIVISION, DBO.GETPHISCALID(), @UOM, @QUANTITY, @RATE, @AMOUNT, @MCODE, @SNO, CONVERT(FLOAT, GETDATE()))";
                        await conn.ExecuteAsync(strOrderMain, s, transaction: tran);
                        await conn.ExecuteAsync(strOrderProd, s.ItemList, transaction: tran);
                        //await sqlcon.ExecuteAsync($"update RMD_SEQUENCES set CurNo=CurNo+1  where VNAME='SALESORDER'  AND DIVISION = '{o.DIVISION}' ;", transaction: tran);
                        tran.Commit();
                        return new { status = "ok", result =$"VCHRNO:'{s.VCHRNO}'", message="Success" };
                        //sqlcon.Close();
                    }
                }
            }
            catch (Exception Ex)
            {
                return new { status = "error", result = Ex.GetBaseException().Message };
            }
        }
        private async Task<string> GETNEWSEQUENCES(string VNAME, SqlTransaction tran, string DIVISION = "", string PhiscalID = "")
        {
            try
            {
                string VoucherType;
                string curno = await tran.Connection.ExecuteScalarAsync<string>($"UPDATE RMD_SEQUENCES SET CurNo=CurNo+1 OUTPUT DELETED.CurNo  WHERE VNAME = '{VNAME}'" +
                    ((VNAME == "Salesorder") ? "" : $" AND DIVISION = '{DIVISION}'"), transaction: tran);
                if (string.IsNullOrEmpty(curno))
                {
                    if (VNAME == "Salesorder")
                        VoucherType = "PA";
                    else
                        VoucherType = "SO";
                    string seqstr = @"INSERT INTO RMD_SEQUENCES
                                      ([VNAME],[CurNo],[DIVISION],[VoucherType],[DIV])
                                      VALUES(@VNAME,1, @DIVISION,@VoucherType,@DIVISION);";
                    await tran.Connection.ExecuteAsync(seqstr, new { VNAME, DIVISION, VoucherType }, transaction: tran);
                    curno = "1";
                }
                return curno;
            }
            catch (Exception Ex)
            {
                return Ex.GetBaseException().Message;
            }
        }
    }
}

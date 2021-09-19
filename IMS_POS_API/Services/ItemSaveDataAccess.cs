using IMS_POS_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using IMS_POS_API.Models;
using IMS_POS_API.Helper;

namespace IMS_POS_API.Services
{

    public class ItemSaveDataAccess
    {
        private IDbConnection db;
        public BarCode BCode;
        public MULTIALTUNIT AltUnit;
        public bool ManualMcode = true;


        public ItemSaveDataAccess(IConfiguration config)
        {
            this.db = new SqlConnection(config.GetConnectionString("myConnectionString"));
        }
        public async Task<dynamic> SaveGroup(List<ItemGroup> param)
        {
            try
            {
                
                IList<FunctionResponse> ErrorItemList = new List<FunctionResponse>();
                IList<ItemGroup> ItemGroupList = param;
                //TrnMain.TRNDATE = trnmainDTO.TransactionDate.Date;
                List<ErrorList> elist = new List<ErrorList>();
                FunctionResponse res = GetExistingProductMCode(ItemGroupList.Select(x => "PRG" + x.GroupCode).ToList(), "G");
                if (res.status != "ok")
                {
                    return new BadRequestObjectResult(res);
                }

                foreach (ItemGroup grp in ItemGroupList)
                {
                    
                    DateTime trndate = db.ExecuteScalar<DateTime>("select getdate()");
                    TimeSpan date = trndate - grp.TimeStamp;
                    if (trndate.Subtract(grp.TimeStamp).TotalMinutes > 5)
                    {

                        throw new Exception("time limit is not valid");
                    }
                    Product product = new Product()
                    {
                        // PTYPE = 0,
                        FCODE = 0,
                        MCODE = grp.GroupCode,
                        DESCA = grp.Name,
                        DESCB = string.Empty,
                        MENUCODE = grp.GroupCode,
                        TYPE = "G",
                        MGROUP = "PRG" + grp.GroupCode,
                        PARENT = "MI",
                        LEVELS = 1,
                        //STATUS = grp.Status,
                        MCAT = "N/A",
                        MCAT1 = "N/A",
                        PATH = "PRODUCT LIST\\" + grp.Name + " - " + grp.GroupCode
                    };
                    string McodeExist = db.ExecuteScalar<string>("select MCODE from MenuItem where MCODE=@MCODE", new { MCODE = product.MCODE });
                    if (!string.IsNullOrEmpty(McodeExist))
                    {
                        elist.Add(new ErrorList { FieldName = "MCODE", ErrorMessage = $"MCODE '{product.MCODE}' is already saved." });
                        return new FunctionResponse { status = "error", result = elist };
                    }
                    if (product.PTYPE < 0)
                        product.PTYPE = 0;
                    if (product.TYPE == "G" && product.PARENT == "MI")
                    {
                        product.MGROUP = product.MCODE;
                        if (!ManualMcode)
                            product.MENUCODE = (product.FCODE > 0) ? product.FCODE.ToString() : string.Empty;
                    }
                    db.Open();
                    SqlTransaction tran = (SqlTransaction)db.BeginTransaction();
                    {
                        try
                        {
                            GetMCode(tran, product);
                            GetMenuCode(tran, product);
                            if (!SaveNewProduct(tran, product))
                                return new FunctionResponse { status = "error", result = "Product could not be saved" };
                            tran.Connection.Execute("UPDATE RMD_SEQUENCES SET CURNO = CURNO+1 WHERE VNAME = 'ITEM'", transaction: tran);
                            // ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Master", "New", VCRHNO: product.MCODE);
                            tran.Commit();
                            return new FunctionResponse { status = "ok", result = new { mcode = product.MCODE, menucode = product.MENUCODE } };

                        }
                        catch (Exception Ex)
                        {

                            throw Ex;
                        }
                    }
                }
                return new FunctionResponse { status = "error", result = "Something went wrong. Please try again." };
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public async Task<dynamic> SaveItem(List<Item> param)
        {
            try
            {
                IList<FunctionResponse> ErrorItemList = new List<FunctionResponse>();
                IList<Item> ItemList = param;
                List<ErrorList> elist = new List<ErrorList>();
                FunctionResponse res = GetExistingProductMCode(ItemList.Select(x => x.SkuCode).ToList(), "A");
                if (res.status != "ok")
                {
                    //_con.SetRequestLog(Request, (JArray)param, res, "ItemSaveRequest");
                    return new BadRequestObjectResult(res);
                }
                IList<string> ExistingItems = res.result as IList<string>;
                res = GetItemParent(ItemList.Select(x => "PRG" + x.GroupCode).Distinct().ToList());
                if (res.status != "ok")
                {
                    return new BadRequestObjectResult(res);
                }
                IList<Product> Parents = res.result as IList<Product>;
                foreach (Item item in ItemList)
                {
                    DateTime trndate = db.ExecuteScalar<DateTime>("select getdate()");
                    TimeSpan date = trndate - item.TimeStamp;
                    if (trndate.Subtract(item.TimeStamp).TotalMinutes > 5)
                    {

                        throw new Exception("time limit is not valid");
                    }
                    if (!Parents.Any(x => x.MENUCODE == item.GroupCode))
                    {
                        ErrorItemList.Add(new FunctionResponse { status = "error", result = string.Format("GroupCode '{0}' of Item '{1}({2})' doesnot exists in IMS Database. Please contact CRM Team.", item.GroupCode, item.Name, item.SkuCode) });
                        continue;
                    }  
                    Product Parent = Parents.FirstOrDefault(x => x.MENUCODE == item.GroupCode);
                    var product = new Product()
                    {
                        MCODE = item.SkuCode,
                        MENUCODE = item.SkuCode,
                        BCODE = item.BarCode,
                        DESCA = item.Name.Trim(),
                        BASEUNIT = string.IsNullOrEmpty(item.UOM) ? "PC" : item.UOM,
                        ALTUNIT = item.AlternateUOM,
                        CONFACTOR = Convert.ToDecimal(item.AlternateQuantity),
                        DESCB = string.Empty,
                        TYPE = "A",
                        MGROUP = "PRG" + item.GroupCode,
                        PARENT = "PRG" + item.GroupCode,
                        LEVELS = 2,
                        PTYPE = 0,
                        //PRATE_A = item.MRP,
                        RATE_A = Convert.ToDecimal(item.MRP),
                        // RATE_B = item.DRate,
                        VAT = item.IsVatItem,
                        STATUS = item.Status,
                        //MCAT = item.Brand,
                        //BRAND = item.Brand,
                        MCAT1 = "N/A",
                        PATH = "PRODUCT LIST\\" + Parent.DESCA + " - " + Parent.MENUCODE + "\\" + item.Name + " - " + item.SkuCode
                    };
                    string McodeExist = db.ExecuteScalar<string>("select MCODE from MenuItem where MCODE=@MCODE", new { MCODE = item.SkuCode });
                    if (!string.IsNullOrEmpty(McodeExist))
                    {
                        elist.Add(new ErrorList { FieldName = "MCODE", ErrorMessage = $"MCODE '{item.SkuCode}' is already saved." });
                        return new FunctionResponse { status = "error", result = elist };
                    }
                    var BCodeList = new List<BarCode>();
                    if (!string.IsNullOrEmpty(item.BarCode))
                        BCodeList.Add(new BarCode { MCODE = item.SkuCode, BCODE = item.BarCode });
                    else
                        BCodeList.Add(new BarCode { MCODE = item.SkuCode, BCODE = item.SkuCode });
                    var AltUnit = new List<MULTIALTUNIT>();
                    if (!string.IsNullOrEmpty(item.AlternateUOM))
                        AltUnit.Add(new MULTIALTUNIT { MCODE = item.SkuCode, ALTUNIT = item.AlternateUOM, CONFACTOR = Convert.ToInt32(item.AlternateQuantity), RATE = Convert.ToInt32(item.AlternateMRP),BRCODE=(item.BarCode!=null?item.BarCode:item.SkuCode) });
                    //else
                    //    AltUnit.Add(new MULTIALTUNIT {MCODE=item.SkuCode,ALTUNIT=item.UOM,CONFACTOR=Convert.ToInt32(item.) })
                    if (product.PTYPE < 0)
                        product.PTYPE = 0;
                    //if (product.TYPE == "G" && product.PARENT == "MI")
                    //{
                    //    product.MGROUP = product.MCODE;
                    //    if (!ManualMcode)
                    //        product.MENUCODE = (product.FCODE > 0) ? product.FCODE.ToString() : string.Empty;
                    //}
                    db.Open();
                    using (SqlTransaction tran = (SqlTransaction)db.BeginTransaction())
                    {
                        try
                        {
                            GetMCode(tran, product);
                            GetMenuCode(tran, product);
                            if (!SaveNewProduct(tran, product))
                                return new FunctionResponse { status = "error", result = "Product could not be saved" };
                            if (product.TYPE == "A")
                            {
                                if (!SaveBarcodes(tran, product, BCodeList))
                                    return new FunctionResponse { status = "error", result = "Barcode could not be saved" };
                                if (!SaveAlternateUnits(tran, product, AltUnit))
                                    return new FunctionResponse { status = "error", result = "Alternate units could not be saved" };
                            }
                            tran.Connection.Execute("UPDATE RMD_SEQUENCES SET CURNO = CURNO+1 WHERE VNAME = 'ITEM'", transaction: tran);
                            // ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Master", "New", VCRHNO: product.MCODE);
                            tran.Commit();
                            return new FunctionResponse { status = "ok", result = new { mcode = product.MCODE, menucode = product.MENUCODE } };

                        }
                        catch (Exception Ex)
                        {

                            throw Ex;
                        }
                    }
                }
                return new FunctionResponse { status = "error", result = "Something went wrong. Please try again." };

            }
            catch (Exception)
            {

                throw;
            }
        }
        public FunctionResponse GetExistingProductMCode(IList<string> MCodeList, string Type = "A")
        {
            try
            {
                string TypeMismatchItems = string.Empty;
                string InStr = "'" + MCodeList[0] + "'";
                for (int i = 1; i < MCodeList.Count; i++)
                {
                    InStr += ", '" + MCodeList[i] + "'";
                }
                IEnumerable<Product> ExsitingMCodeList = db.Query<Product>("SELECT MCODE, TYPE FROM MENUITEM WHERE MCODE IN (" + InStr + ")");
                foreach (Product item in ExsitingMCodeList)
                {
                    if (item.TYPE != Type)
                        TypeMismatchItems += item.MCODE + ", ";
                }
                if (!string.IsNullOrEmpty(TypeMismatchItems))
                {
                    if (Type == "G")
                        return new FunctionResponse { status = "error", result = "There are items with codes " + TypeMismatchItems };
                    else
                        return new FunctionResponse { status = "error", result = "There are item groups with codes " + TypeMismatchItems };
                }
                return new FunctionResponse { status = "ok", result = ExsitingMCodeList.Select(x => x.MCODE).ToList<string>() };

            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
        }
        public void GetMCode(SqlTransaction tran, Product product)
        {
            try
            {
                if (!ManualMcode)
                {
                    tran.Connection.Execute(
                        @"IF NOT EXISTS (SELECT * FROM RMD_SEQUENCES WHERE VNAME = 'ITEM')
                        INSERT INTO RMD_SEQUENCES(VNAME, CurNo) VALUES('ITEM', 1)", transaction: tran);
                    string Prefix = (product.TYPE == "A") ? "M" : "PRG";

                    product.MCODE = Prefix + tran.Connection.ExecuteScalar(@"SELECT CurNo FROM RMD_SEQUENCES WHERE VNAME = 'ITEM'", transaction: tran).ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetMenuCode(SqlTransaction tran, Product product)
        {
            try
            {
                //if (GlobalSetting.GblManualCode == 0)
                //{
                if (product.TYPE == "A")
                {
                    product.ECODE = Convert.ToDecimal(tran.Connection.ExecuteScalar("SELECT ISNULL(MAX(ECODE),0) + 1 AS EC FROM MENUITEM WHERE FCODE = '" + product.FCODE + "' AND TYPE = 'A'", transaction: tran));
                    product.MENUCODE = product.FCODE.ToString() + "." + product.ECODE.ToString();
                }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool SaveNewProduct(SqlTransaction Tran, Product product)
        {
            string SaveQry;
            if (product.TYPE == "A")
            {
                SaveQry = @"DECLARE @NewProduct TABLE(MCODE VARCHAR(100), MENUCODE VARCHAR(100))
                                    INSERT INTO MENUITEM (ALTUNIT, BARCODE, BASEUNIT, BRAND, CBM, COLOR, CONFACTOR, CRATE, CRDATE, 
                                    DESCA ,DESCB, DIMENSION, DISAMOUNT, DISCONTINUE, DISMODE, DISRATE, 
                                    ECODE, EDATE, FCODE, FLGNEW, FOB, GWEIGHT, HASBATCH, HASSERIAL, HASSERVICECHARGE, ISBARITEM, ISUNKNOWN, LEVELS, LPDATE, 
                                    MARGIN, MAXLEVEL, MAXSQTY, MAXWARN, MCAT, MCAT1, MCODE, MENUCODE, MGROUP, MIDCODE, MINLEVEL, MINWARN, MODEL, MODES, NWEIGHT, 
                                    PAC, PACK, PARENT, PATH, PRAC, PRATE_A, PRATE_B, PRERATE, PRERATE1, PRERATE2, PRESRATE, PRODTYPE, PTYPE, 
                                    RATE_A, RATE_B, RATE_C, RECRATE, REQEXPDATE, ROLEVEL, ROWARN, 
                                    SAC, SALESMANID, SCHEME_A, SCHEME_B, SCHEME_C, SCHEME_D, SCHEME_E, SRAC, SUPCODE, SUPITEMCODE, 
                                    TAXGROUP_ID, TDAILY, TMONTHLY, TRNUSER, TSTOP, TYEARLY, TYPE,  
                                    VAT, VPRATE, VSRATE, WHOUSE, STAMP) OUTPUT INSERTED.MCODE,INSERTED.MENUCODE INTO @NewProduct
                                    
                                    SELECT @ALTUNIT, @BARCODE, @BASEUNIT, @BRAND, @CBM, @COLOR, @CONFACTOR, @CRATE, @CRDATE, 
                                    @DESCA , @DESCB, @DIMENSION, ISNULL(@DISAMOUNT, 0), @DISCONTINUE, ISNULL(@DISMODE,'DISCOUNTABLE'), ISNULL(@DISRATE, 0), 
                                    @ECODE, GETDATE(), A.FCODE, @FLGNEW, @FOB, @GWEIGHT, @HASBATCH, @HASSERIAL, @HASSERVICECHARGE, @ISBARITEM, @ISUNKNOWN, A.LEVELS +1, @LPDATE, 
                                    @MARGIN, @MAXLEVEL, @MAXSQTY, @MAXWARN, ISNULL(@MCAT, ''), ISNULL(@MCAT1, ''), @MCODE, @MENUCODE, A.MGROUP, @MIDCODE, @MINLEVEL, @MINWARN, @MODEL, @MODES,  @NWEIGHT, 
									@PAC, @PACK, @PARENT, A.PATH + '\' + @DESCA, @PRAC, @PRATE_A, @PRATE_B, @PRERATE, @PRERATE1, @PRERATE2, @PRESRATE, @PRODTYPE, @PTYPE, 
                                    @RATE_A, @RATE_B, @RATE_C, A.RECRATE, @REQEXPDATE, @ROLEVEL, @ROWARN, 
                                    @SAC, @SALESMANID, A.SCHEME_A , A.SCHEME_B , A.SCHEME_C , A.SCHEME_D , A.SCHEME_E , @SRAC, @SUPCODE, @SUPITEMCODE,                                                                         
                                    ISNULL(@TAXGROUP_ID,0), @TDAILY, @TMONTHLY, 'admin', @TSTOP, @TYEARLY, @TYPE,  
                                    @VAT, @VPRATE, @VSRATE, @WHOUSE, @STAMP FROM MENUITEM A WHERE MCODE=@PARENT
                                    SELECT * FROM @NewProduct";
            }
            else
            {
                SaveQry = @"DECLARE @NewProduct TABLE(MCODE VARCHAR(100), MENUCODE VARCHAR(100))
                                    INSERT INTO MENUITEM (CRATE, CRDATE, DESCA ,DESCB, DISAMOUNT, DISCONTINUE, DISMODE, DISRATE, ECODE, EDATE, FCODE, FLGNEW, FOB, 
                                    HASBATCH, ISUNKNOWN, LEVELS, LPDATE, 
                                    MARGIN, MAXLEVEL, MAXWARN, MCAT, MCAT1, MCODE, MENUCODE, MGROUP, MIDCODE, MINLEVEL, MINWARN, MODES, 
                                    PAC, PACK, PARENT, PATH, PRAC, PRATE_A, PRATE_B, PRERATE, PRERATE1, PRERATE2, PRESRATE, PRODTYPE, PTYPE, 
                                    RATE_A, RATE_B, RATE_C, RECRATE, ROLEVEL, ROWARN,                                      
                                    SAC, SALESMANID, SCHEME_A, SCHEME_B, SCHEME_C, SCHEME_D, SCHEME_E, SRAC, 
                                    TAXGROUP_ID, TDAILY, TMONTHLY, TSTOP, TYEARLY, TYPE, 
                                    VAT, VPRATE, VSRATE, WHOUSE, STAMP) OUTPUT INSERTED.MCODE,INSERTED.MENUCODE INTO @NewProduct
                                    VALUES
                                    (@CRATE, @CRDATE, @DESCA , @DESCB, @DISAMOUNT, @DISCONTINUE, @DISMODE, @DISRATE, @ECODE, @EDATE, @FCODE, @FLGNEW, @FOB, 
                                    @HASBATCH, @ISUNKNOWN, @LEVELS, @LPDATE, 
                                    @RECRATE,  @MAXLEVEL, @MAXWARN, @MCAT, @MCAT1, @MCODE, @MENUCODE, @MGROUP, @MIDCODE, @MINLEVEL, @MINWARN, @MODES, 
                                    @PAC, @PACK, @PARENT, @PATH, @PRAC, @PRATE_A, @PRATE_B, @PRERATE, @PRERATE1, @PRERATE2, @PRESRATE, @PRODTYPE, @PTYPE, 
                                    @RATE_A, @RATE_B, @RATE_C, @RECRATE, @ROLEVEL, @ROWARN, 
                                    @SAC, @SALESMANID, @SCHEME_A, @SCHEME_B, @SCHEME_C, @SCHEME_D, @SCHEME_E, @SRAC, 
                                    @TAXGROUP_ID, @TDAILY, @TMONTHLY, @TSTOP, @TYEARLY, @TYPE, 
                                    @VAT, @VPRATE, @VSRATE, @WHOUSE, @STAMP)
                                    SELECT * FROM @NewProduct";
            }
            var retTable = Tran.Connection.Query(SaveQry, product, transaction: Tran);
            if (retTable != null)
            {
                var row = retTable.ToList()[0];
                IDictionary<string, object> ColumnValue = row as IDictionary<string, object>;
                product.MCODE = ColumnValue["MCODE"].ToString();
                product.MENUCODE = ColumnValue["MENUCODE"] == null ? "" : ColumnValue["MENUCODE"].ToString();
                return true;
            }
            //return Tran.Connection.Execute(cd) > 0;
            return false;
        }

        //BarCode Region
        public bool SaveBarcodes(SqlTransaction tran, Product product, List<BarCode> BCodeList)
        {
            if (BCodeList != null && BCodeList.Count > 0)
            {
                tran.Connection.Execute("DELETE FROM BARCODE WHERE MCODE = '" + product.MCODE + "'", transaction: tran);
                foreach (BarCode b in BCodeList)
                {
                    b.MCODE = product.MCODE;
                    this.BCode = b;
                    if (SaveBarcode(tran))
                    {
                        if (BCode.BCodeDetails != null && BCode.BCodeDetails.Count > 0 && !SaveBarCodeDetail(tran))
                            return false;
                    }
                    else
                        return false;
                }
            }
            return true;
        }
        public bool SaveBarcode(SqlTransaction tran)
        {
            string SaveQuery = @"INSERT INTO BARCODE(BCODE, MCODE, UNIT, ISSUENO, EDATE, BCODEID, SUPCODE, BATCHNO, EXPIRY, INVNO, DIV, FYEAR, SRATE, REMARKS)
                                        VALUES (@BCODE, @MCODE, @UNIT, @ISSUENO, @EDATE, (SELECT MAX(BCODEID) + 1 BCODEID from BARCODE), @SUPCODE, @BATCHNO, @EXPIRY, @INVNO, @DIV, @FYEAR, @SRATE, @REMARKS)";
            return tran.Connection.Execute(SaveQuery, BCode, tran) == 1;

        }
        public bool SaveBarCodeDetail(SqlTransaction tran)
        {
            tran.Connection.Execute("DELETE FROM BARCODE_DETAIL WHERE BARCODE = @BARCODE AND MCODE = @MCODE", new { BARCODE = BCode.BCODE, MCODE = BCode.MCODE }, tran);
            string SaveQuery = "INSERT INTO BARCODE_DETAIL(BARCODE, MCODE";
            string Values = " VALUES ('" + BCode.BCODE + "', '" + BCode.MCODE + "'";
            foreach (var kv in BCode.BCodeDetails)
            {
                SaveQuery += ", " + kv.COLUMN_NAME;
                switch (kv.DATA_TYPE)
                {
                    case "varchar":
                        Values += (kv.VALUE == null || kv.VALUE.Equals(DBNull.Value)) ? ", NULL" : ", '" + kv.VALUE + "'";
                        break;
                    case "datetime":
                        Values += (kv.VALUE == null || kv.VALUE.Equals(DBNull.Value)) ? ", NULL" : ", '" + ((DateTime)kv.VALUE).ToString("MM/dd/yyyy") + "'";
                        break;
                    case "date":
                        Values += (kv.VALUE == null || kv.VALUE.Equals(DBNull.Value)) ? ", NULL" : ", '" + ((DateTime)kv.VALUE).ToString("MM/dd/yyyy") + "'";
                        break;
                    default:
                        Values += (kv.VALUE == null || kv.VALUE.Equals(DBNull.Value)) ? ", NULL" : ", " + kv.VALUE;
                        break;
                }
            }
            SaveQuery += ") " + Values + ")";
            return tran.Connection.Execute(SaveQuery, transaction: tran) == 1;
        }
        public Task<FunctionResponse> GetProductBarcodeByMCode(string MCODE)
        {
            try
            {
                var BCodeList = db.Query<BarCode>("SELECT BCODE, MCODE, UNIT, ISSUENO, EDATE, BCODEID, SUPCODE, BATCHNO, EXPIRY, INVNO, DIV, FYEAR, SRATE, REMARKS FROM BarCode WHERE MCODE='" + MCODE + "'");
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "ok", result = BCodeList });
            }
            catch (Exception ex)
            {
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "error", result = ex.Message });
            }
        }

        public IEnumerable<BarcodeDetail> GetBarcodeDetails(string MCODE, string BCODE)
        {
            IEnumerable<BarcodeDetail> BCodeDetails;
            BCodeDetails = db.Query<BarcodeDetail>("SELECT C.COLUMN_NAME, DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) [COL_LENGTH] FROM INFORMATION_SCHEMA.COLUMNS C JOIN BARCODE_DETAIL_FIELDS BDF ON C.COLUMN_NAME = BDF.COLUMN_NAME WHERE TABLE_NAME = 'BARCODE_DETAIL' AND ORDINAL_POSITION > 2 AND IsEnabled = 1");
            using (var reader = db.ExecuteReader("SELECT * FROM BARCODE_DETAIL WHERE MCODE='" + MCODE + "' AND BARCODE = '" + BCODE + "'"))
            {
                while (reader.Read())
                {
                    foreach (BarcodeDetail BCode in BCodeDetails)
                    {
                        BCode.VALUE = reader[BCode.COLUMN_NAME];
                    }
                }
            }
            return BCodeDetails;
        }

        //Alternate Unit
        public bool SaveAlternateUnits(SqlTransaction Tran, Product product, List<MULTIALTUNIT> AltUnit)
        {
            if (AltUnit != null && AltUnit.Count() > 0)
            {
                Tran.Connection.Execute("DELETE FROM MULTIALTUNIT WHERE MCODE = '" + product.MCODE + "'", transaction: Tran);
                foreach (MULTIALTUNIT au in AltUnit)
                {
                    this.AltUnit = au;
                    this.AltUnit.MCODE = product.MCODE;
                    if (!SaveAlternateUnit(Tran))
                        return false;
                }
            }
            return true;
        }
        public bool SaveAlternateUnit(SqlTransaction tran)
        {
            return tran.Connection.Execute("INSERT INTO MULTIALTUNIT(MCODE, ALTUNIT, CONFACTOR, RATE, ISDEFAULT, BRCODE, PRATE, ISDISCONTINUE, ISDEFAULTPRATE) VALUES (@MCODE, @ALTUNIT, @CONFACTOR, @RATE, @ISDEFAULT, @BRCODE, @PRATE, @ISDISCONTINUE, @ISDEFAULTPRATE)", AltUnit, tran) == 1;
        }
        public FunctionResponse GetItemParent(IList<string> MCodeList)
        {
            try
            {
                string InStr = "'" + MCodeList[0] + "'";
                for (int i = 1; i < MCodeList.Count; i++)
                {
                    InStr += ", '" + MCodeList[i] + "'";
                }
                    IEnumerable<Product> ParentList = db.Query<Product>("SELECT MCODE, DESCA, MENUCODE, MCAT FROM MENUITEM WHERE MCODE IN (" + InStr + ")");
                    return new FunctionResponse { status = "ok", result = ParentList.ToList() };
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
        }
    }
}


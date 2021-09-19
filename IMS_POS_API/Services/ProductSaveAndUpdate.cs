using ClientApi.Controllers;
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


namespace IMS_POS_API.Services
{
    public class ProductSaveAndUpdate:BaseDataAccess
    {
        
        public Product product;
        private Barcode BCode;
        private ItemRate IRate;
        private AlternateUnit AltUnit;
        private IList<Barcode> BCodeList;
        private IList<AlternateUnit> AltUnitList;
        private IList<ItemRate> IRateList;
        public IList<BrandModel> BMList;
        public bool ManualMcode = false;

        
        public ProductSaveAndUpdate(Product _Product = null, Barcode _Barcode = null, ItemRate _ItemRate = null, AlternateUnit _AltUnit = null, IList<Barcode> _BCodeList = null, IList<ItemRate> _IRateList = null, IList<AlternateUnit> _AltUnitList = null, IList<BrandModel> _BMList = null)
        {
            product = _Product;
            BCode = _Barcode;
            IRate = _ItemRate;
            AltUnit = _AltUnit;
            BCodeList = _BCodeList;
            AltUnitList = _AltUnitList;
            IRateList = _IRateList;
            BMList = _BMList;
        }

        public FunctionResponse SaveAll()
        {
            try
            {
                if (product.PTYPE < 0)
                    product.PTYPE = 0;
                if (product.TYPE == "G" && product.PARENT == "MI")
                {
                    product.MGROUP = product.MCODE;
                    if (!ManualMcode)
                        product.MENUCODE = (product.FCODE > 0) ? product.FCODE.ToString() : string.Empty;
                }
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            GetMCode(tran);
                            GetMenuCode(tran);
                            if (!SaveNewProduct(tran))
                                return new FunctionResponse { status = "error", result = "Product could not be saved" };
                            if (product.TYPE == "A")
                            {
                                //if(!SaveMultiStockLevels(tran))
                                //    return new FunctionResponse { status = "error", result = "MultiStockLevel could not be saved" };
                                if (!SaveAlternateUnits(tran))
                                    return new FunctionResponse { status = "error", result = "Alternate units could not be saved" };
                                if (!SaveBarcodes(tran))
                                    return new FunctionResponse { status = "error", result = "Barcode could not be saved" };
                                //if (!SaveItemRates(tran))
                                //    return new FunctionResponse { status = "error", result = "Item Rate could not be saved" };
                                //if (GlobalSetting.GblEnableRateDiscount == 1 && !SaveRateDiscount(tran))
                                //    return new FunctionResponse { status = "error", result = "Rate Discount could not be saved" };
                                //if (!SaveBrandModels(tran))
                                //    return new FunctionResponse { status = "error", result = "Brand/Model could not be saved" };
                            }
                            tran.Connection.Execute("UPDATE RMD_SEQUENCES SET CURNO = CURNO+1 WHERE VNAME = 'ITEM'", transaction: tran);
                            //ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Master", "New", VCRHNO: product.MCODE);
                            tran.Commit();
                            return new FunctionResponse { status = "ok", result = new {mcode=product.MCODE,menucode=product.MENUCODE } };
                        }
                        catch (Exception Ex)
                        {
                            return new FunctionResponse { status = "error", result =Ex.GetBaseException().Message };
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = GlobalClass.GetRootException(Ex).Message };
            }
        }



        public FunctionResponse UpdateAll()
        {
            try
            {
                if (product.PTYPE < 0)
                    product.PTYPE = 0;
                if (product.TYPE == "G" && product.PARENT == "MI")
                {
                    product.MGROUP = product.MCODE;
                    if (!ManualMcode)
                        product.MENUCODE = product.FCODE.ToString();
                }
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            if (!UpdateProduct(tran))
                                return new FunctionResponse { status = "error", result = "Product could not be saved" };
                            if (product.TYPE == "A")
                            {

                                if (!SaveMultiStockLevels(tran))
                                    return new FunctionResponse { status = "error", result = "MultiStockLevel could not be saved" };
                                if (!SaveAlternateUnits(tran))
                                    return new FunctionResponse { status = "error", result = "Alternate units could not be saved" };
                                if (!SaveBarcodes(tran))
                                    return new FunctionResponse { status = "error", result = "Barcode could not be saved" };
                                if (!SaveItemRates(tran))
                                    return new FunctionResponse { status = "error", result = "Item Rate could not be saved" };
                                if (GlobalSetting.GblEnableRateDiscount == 1)
                                    UpdateRateDiscount(tran);
                                if (!SaveBrandModels(tran))
                                    return new FunctionResponse { status = "error", result = "Brand/Model could not be saved" };
                                //return "Rate Discount could not be saved";
                            }
                            ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Master", "Edit", VCRHNO: product.MCODE);
                            tran.Commit();
                            return new FunctionResponse { status = "ok", result = new { mcode = product.MCODE, menucode = product.MENUCODE } };
                        }
                        catch (Exception Ex)
                        {
                            return new FunctionResponse { status = "error", result = GlobalClass.GetRootException(Ex).Message };
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = GlobalClass.GetRootException(Ex).Message };
            }
        }

        public FunctionResponse DeleteProduct()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        conn.Execute("DELETE FROM MultiBrandModel WHERE MCODE = @MCODE", product, tran);
                        conn.Execute("DELETE FROM MENUITEM WHERE MCODE = @MCODE", product, tran);
                        ActivityLog.SetUserActivityLog(product.TRNUSER, tran, Session.SessionId, "Product Master", "Delete", VCRHNO: product.MCODE);
                        tran.Commit();
                    }
                    return new FunctionResponse() { status = "ok" };
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse() { status = "error", result = ex.Message };
            }
        }

        #region MenuItem
        public bool SaveNewProduct(SqlTransaction Tran)
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
            DynamicParameters dp = GlobalClass.GetParamters(product);
            //dp.Add("@OutMcode", dbType: DbType.String, direction: ParameterDirection.Output);
            //CommandDefinition cd = new CommandDefinition(SaveQry, GlobalClass.GetParamters(product), Tran);
            var retTable = Tran.Connection.Query(SaveQry, dp, transaction: Tran);
            if (retTable != null)
            {
                var row = retTable.ToList()[0];
                IDictionary<string, object> ColumnValue = row as IDictionary<string, object>;
                product.MCODE = ColumnValue["MCODE"].ToString();
                product.MENUCODE = ColumnValue["MENUCODE"]==null?"": ColumnValue["MENUCODE"].ToString();
                return true;
            }
            //return Tran.Connection.Execute(cd) > 0;
            return false;
        }

        public bool UpdateProduct(SqlTransaction Tran)
        {
            if (product.TYPE == "G")
                product.MARGIN = product.RECRATE;
            string UpdateQry = @"UPDATE MENUITEM SET ALTUNIT = @ALTUNIT, BARCODE = @BARCODE, BASEUNIT = @BASEUNIT, BRAND = @BRAND, CBM = @CBM, COLOR = @COLOR, CONFACTOR = @CONFACTOR, 
                                    CRATE = @CRATE, CRDATE = @CRDATE, DESCA = @DESCA, DESCB = @DESCB, DIMENSION = @DIMENSION, DISCONTINUE = @DISCONTINUE, DISMODE = @DISMODE, 
                                    DISRATE = @DISRATE, DISAMOUNT =@DISAMOUNT, ECODE = @ECODE, EDATE = GetDate(), FCODE = @FCODE, FLGNEW = @FLGNEW, FOB = @FOB, GENERIC = @GENERIC, 
                                    GWEIGHT = @GWEIGHT, HASBATCH = @HASBATCH, HASSERIAL = @HASSERIAL, HASSERVICECHARGE = @HASSERVICECHARGE, ISBARITEM = @ISBARITEM, ISUNKNOWN = @ISUNKNOWN, 
                                    LATESTBILL = @LATESTBILL, LEVELS = @LEVELS, LPDATE = @LPDATE, MARGIN = @MARGIN, MAXLEVEL = @MAXLEVEL, MAXSQTY = @MAXSQTY, MAXWARN = @MAXWARN, 
                                    MCAT = @MCAT, MCAT1 = @MCAT1, MENUCODE = @MENUCODE, MGROUP = @MGROUP, MIDCODE = @MIDCODE, MINWARN = @MINWARN, MINLEVEL = @MINLEVEL, MODEL = @MODEL, 
                                    MODES = @MODES, NWEIGHT = @NWEIGHT, PAC = @PAC, PACK = @PACK, PARENT = @PARENT, PATH = @PATH, PRAC = @PRAC, PRATE_A = @PRATE_A, PRATE_B = @PRATE_B, 
                                    PRERATE = @PRERATE, PRERATE1 = PRERATE1, PRERATE2 = PRERATE2, PRESRATE = @PRESRATE, PRODTYPE = @PRODTYPE, PTYPE = @PTYPE, RATE_A = @RATE_A, 
                                    RATE_B = @RATE_B, RATE_C = @RATE_C, RECRATE = @RECRATE, ROLEVEL = @ROLEVEL, ROWARN = @ROWARN, SAC = @SAC, SALESMANID = @SALESMANID, 
                                    SCHEME_A = @SCHEME_A, SCHEME_B = @SCHEME_B, SCHEME_C = @SCHEME_C, SCHEME_D = @SCHEME_D, SCHEME_E = @SCHEME_E, SRAC = @SRAC, SUPCODE = @SUPCODE, 
                                    SUPITEMCODE = @SUPITEMCODE, TAXGROUP_ID = @TAXGROUP_ID, TDAILY = @TDAILY, TMONTHLY = @TMONTHLY, TSTOP = @TSTOP, [TYPE] = @TYPE, TYEARLY = @TYEARLY, 
                                    VAT = @VAT, VPRATE = @VPRATE, VSRATE = @VSRATE, WHOUSE = @WHOUSE, ZEROROLEVEL = @ZEROROLEVEL WHERE MCODE = @MCODE";
            return Tran.Connection.Execute(UpdateQry, GlobalClass.GetParamters(product), Tran) >= 1;
        }
        #endregion

        #region Barcode
        public bool SaveBarcodes(SqlTransaction tran)
        {
            if (BCodeList != null && BCodeList.Count > 0)
            {
                tran.Connection.Execute("DELETE FROM BARCODE WHERE MCODE = '" + product.MCODE + "'", transaction: tran);
                foreach (Barcode b in BCodeList)
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
            return tran.Connection.Execute(SaveQuery, GlobalClass.GetParamters(BCode), tran) == 1;

        }

        public bool UpdateBarcode(SqlTransaction tran)
        {
            string SaveQuery = @"UPDATE BARCODE SET BCODE = @BCODE, UNIT = @UNIT, ISSUENO = @ISSUENO, EDATE = @EDATE, SUPCODE = @SUPCODE, BATCHNO = @BATCHNO, EXPIRY = @EXPIRY, 
                                        INVNO = @INVNO, SRATE = @SRATE WHERE MCODE = @MCODE AND BCODEID = @BCODEID AND ISNULL(DIV,'') = @DIV AND ISNULL(FYEAR,'') = @FYEAR";
            return tran.Connection.Execute(SaveQuery, GlobalClass.GetParamters(BCode), tran) > 0;
        }

        public bool DeleteBarcode(SqlTransaction tran)
        {
            string DeleteQuery = "DELETE FROM BARCODE WHERE BCODE = '" + BCode.BCODE + "'";
            return tran.Connection.Execute(DeleteQuery, transaction: tran) > 0;
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
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    var BCodeList = conn.Query<Barcode>("SELECT BCODE, MCODE, UNIT, ISSUENO, EDATE, BCODEID, SUPCODE, BATCHNO, EXPIRY, INVNO, DIV, FYEAR, SRATE, REMARKS FROM BarCode WHERE MCODE='" + MCODE + "'");
                    return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "ok", result = BCodeList });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "error", result = ex.Message });
            }
        }

        public IEnumerable<BarcodeDetail> GetBarcodeDetails(string MCODE, string BCODE)
        {
            IEnumerable<BarcodeDetail> BCodeDetails;
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                BCodeDetails = conn.Query<BarcodeDetail>("SELECT C.COLUMN_NAME, DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) [COL_LENGTH] FROM INFORMATION_SCHEMA.COLUMNS C JOIN BARCODE_DETAIL_FIELDS BDF ON C.COLUMN_NAME = BDF.COLUMN_NAME WHERE TABLE_NAME = 'BARCODE_DETAIL' AND ORDINAL_POSITION > 2 AND IsEnabled = 1");
                using (var reader = conn.ExecuteReader("SELECT * FROM BARCODE_DETAIL WHERE MCODE='" + MCODE + "' AND BARCODE = '" + BCODE + "'"))
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
        }
        #endregion

        #region ItemRate
        public bool SaveItemRates(SqlTransaction tran)
        {
            if (IRateList != null && IRateList.Count > 0)
            {
                tran.Connection.Execute("DELETE FROM ITEMRATE WHERE MCODE = '" + product.MCODE + "'", transaction: tran);
                if (IRateList != null && IRateList.Count > 0)
                {
                    foreach (ItemRate ir in IRateList)
                    {
                        IRate = ir;
                        IRate.MCODE = product.MCODE;
                        if (ir.ExistsInCollection || ir.RATEID == 0)
                            continue;
                        if (!SaveItemRate(tran))
                            return false;
                    }
                }
            }
            return true;
        }
        public bool SaveItemRate(SqlTransaction tran)
        {
            string SaveSql = "INSERT INTO ITEMRATE(MCODE, RATEID, RATETYPE, SNO, RATE, UNIT, ISNEW) VALUES (@MCODE, @RATEID, @RATETYPE, @SNO, @RATE, @UNIT, @ISNEW)";
            return tran.Connection.Execute(SaveSql, IRate, tran) > 0;
        }
        public Task<FunctionResponse> GetRateGroupListByMCode(string mCODE)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    var mbm = conn.Query<ItemRate>(@"SELECT * FROM ITEMRATE WHERE MCODE = @MCODE", new { MCODE = mCODE });
                    return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "ok", result = mbm });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "error", result = ex.Message });
            }
        }
        #endregion

        #region  ItemRateDiscount
        public bool SaveRateDiscount(SqlTransaction tran)
        {
            if (product.ItemRateDiscount == null)
                return true;
            product.ItemRateDiscount.MCODE = product.MCODE;
            string SaveQuery = @"INSERT INTO ItemRateDiscount (MCODE, DTRRATE, WSLRATE, RTLRATE, FLTRATE) VALUES (@MCODE, @DTRRATE, @WSLRATE, @RTLRATE, @FLTRATE)";
            return tran.Connection.Execute(SaveQuery, GlobalClass.GetParamters(product.ItemRateDiscount), tran) > 0;


        }

        public bool UpdateRateDiscount(SqlTransaction tran)
        {
            string SaveQuery = @"UPDATE ItemRateDiscount SET DTRRATE = @DTRRATE, WSLRATE = @WSLRATE, RTLRATE = @RTLRATE, FLTRATE = @FLTRATE WHERE MCODE = @MCODE";
            return tran.Connection.Execute(SaveQuery, GlobalClass.GetParamters(product.ItemRateDiscount), tran) > 0;
        }

        #endregion

        #region MultiStockLevel
        public bool SaveMultiStockLevels(SqlTransaction Tran)
        {
            Tran.Connection.Execute("DELETE FROM MultiStockLevel WHERE MCODE = '" + product.MCODE + "'", transaction: Tran);
            if (product.MultiStockLevels.Count > 0)
            {
                foreach (MultiStockLevel MSLevel in product.MultiStockLevels)
                {
                    MSLevel.MCODE = product.MCODE;
                    if (!SaveMultiStockLevel(MSLevel, Tran))
                        return false;
                }
            }
            return true;
        }
        public bool SaveMultiStockLevel(MultiStockLevel MSLevel, SqlTransaction tran)
        {
            return tran.Connection.Execute("INSERT INTO MultiStockLevel(MCODE, WAREHOUSE, ROLEVEL, MINLEVEL, MAXLEVEL) VALUES (@MCODE, @WAREHOUSE, @ROLEVEL, @MINLEVEL, @MAXLEVEL)", MSLevel, tran) == 1;
        }
        #endregion
        #region AlternateUnit
        public bool SaveAlternateUnits(SqlTransaction Tran)
        {
            if (AltUnitList != null && AltUnitList.Count > 0)
            {
                Tran.Connection.Execute("DELETE FROM MULTIALTUNIT WHERE MCODE = '" + product.MCODE + "'", transaction: Tran);
                foreach (AlternateUnit au in AltUnitList)
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
        public Task<FunctionResponse> GetAlternateUnitListByMCode(string mCODE)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    var mbm = conn.Query<AlternateUnit>(@"SELECT * FROM MULTIALTUNIT WHERE MCODE = @MCODE", new { MCODE = mCODE });
                    return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "ok", result = mbm });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "error", result = ex.Message });
            }
        }
        #endregion

        #region BrandModel
        public FunctionResponse SaveBrandAsync(Brand brand)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM BRAND WHERE BrandName = @BrandName", brand, tran) > 0)
                        {
                            return new FunctionResponse { status = "error", result = "There is already brand with name '" + brand.BrandName + "'" };
                        }
                        conn.Execute("INSERT INTO BRAND(BrandId, BrandName, Stamp) VALUES (@BrandId, @BrandName, @Stamp)", brand, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Brand", "New", VCRHNO: brand.BrandId.ToString());
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse { status = "error", result = ex.Message };
            }
        }

        public FunctionResponse UpdateBrandAsync(Brand brand)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM BRAND WHERE BrandName = @BrandName AND BrandId <> @BrandId", brand, tran) > 0)
                        {
                            return new FunctionResponse { status = "error", result = "There is already brand with name '" + brand.BrandName + "'" };
                        }
                        conn.Execute("UPDATE BRAND SET BrandName = @BrandName WHERE BrandId = @BrandId", brand, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Brand", "Edit", VCRHNO: brand.BrandId.ToString());
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse { status = "error", result = ex.Message };
            }
        }

        public Brand SaveBrand(Brand brand)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM BRAND WHERE BrandName = @BrandName", brand) > 0)
                {
                    return conn.Query<Brand>("SELECT BrandId, BrandName FROM BRAND WHERE BrandName = @BrandName", brand).FirstOrDefault();
                }
                return conn.Query<Brand>("INSERT INTO BRAND(BrandId, BrandName) OUTPUT INSERTED.BrandId, INSERTED.BrandName VALUES ((SELECT ISNULL(MAX(BrandId),0) + 1 FROM BRAND), @BrandName)", brand).FirstOrDefault();
            }
        }

        public FunctionResponse DeleteBrand(Brand brand)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MultiBrandModel WHERE BrandId = @BrandId", brand, tran) > 0)
                    {
                        return new FunctionResponse { status = "error", result = string.Format("Brand {0} ({1}) is already linked with item. Thus cannot be deleted", brand.BrandName, brand.BrandId) };
                    }
                    conn.Execute("DELETE FROM BRAND WHERE BrandId = @BrandId", brand, tran);
                    ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Brand", "Delete", VCRHNO: brand.BrandId.ToString());
                    tran.Commit();
                    return new FunctionResponse { status = "ok" };
                }
            }
        }


        public FunctionResponse SaveModelAsync(Model model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MODEL WHERE ModelName = @ModelName", model, tran) > 0)
                        {
                            return new FunctionResponse { status = "error", result = "There is already Model with name '" + model.ModelName + "'" };
                        }
                        conn.Execute("INSERT INTO MODEL(ModelId, BrandId, ModelName, Stamp) VALUES(@ModelId, @BrandId, @ModelName, @Stamp)", model, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Model", "New", VCRHNO: model.ModelId.ToString());
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }

            }
            catch (Exception ex)
            {
                return new FunctionResponse
                {
                    status = "error",
                    result = ex.Message
                };
            }
        }

        public FunctionResponse UpdateModelAsync(Model model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MODEL WHERE ModelName = @ModelName AND ModelId <> @ModelId", model, tran) > 0)
                        {
                            return new FunctionResponse { status = "error", result = "There is already Model with name '" + model.BrandName + "'" };
                        }
                        conn.Execute("UPDATE MODEL SET BrandId = @BrandId, ModelName = @ModelName, Stamp = @Stamp WHERE ModelId = @ModelId", model, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Model", "Edit", VCRHNO: model.ModelId.ToString());
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse
                {
                    status = "error",
                    result = ex.Message
                };
            }
        }

        public Model SaveModel(Model model)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MODEL WHERE ModelName = @ModelName", model) > 0)
                {
                    return conn.Query<Model>("SELECT ModelId, BrandId, ModelName FROM MODEL WHERE ModelName = @ModelName", model).FirstOrDefault();
                }
                return conn.Query<Model>("INSERT INTO MODEL(ModelId, BrandId, ModelName) OUTPUT INSERTED.ModelId, INSERTED.BrandId, INSERTED.ModelName VALUES ((SELECT ISNULL(MAX(ModelId),0) + 1 FROM MODEL), @BrandId, @ModelName)", model).FirstOrDefault();
            }
        }

        public FunctionResponse DeleteModel(Model model)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    //if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MultiBrandModel WHERE ModelId = @ModelId", model, tran) > 0)
                    //{
                    //    return new FunctionResponse { status = "error", result = string.Format("Model {0} ({1}) is already linked with item. Thus cannot be deleted", model.ModelName, model.ModelId) };
                    //}
                    conn.Execute("DELETE FROM MultiBrandModel WHERE ModelId = @ModelId", model, tran);
                    conn.Execute("DELETE FROM MODEL WHERE ModelId = @ModelId", model, tran);
                    ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Model", "Delete", VCRHNO: model.ModelId.ToString());
                    tran.Commit();
                    return new FunctionResponse { status = "ok" };
                }
            }
        }

        public FunctionResponse SaveBrandModelAsync(BrandModel bm)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        conn.Execute("INSERT INTO MultiBrandModel(MCODE, BrandId, ModelId, Stamp) SELECT @MCODE, BrandId, @ModelId, @Stamp FROM Model WHERE ModelId = @ModelId", bm, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Master", "Edit", VCRHNO: bm.MCODE);
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse { status = "error", result = ex.Message };
            }
        }
        
        public FunctionResponse DeleteBrandModelAsync(BrandModel bm)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        conn.Execute("DELETE FROM MultiBrandModel WHERE MCODE =  @MCODE AND ModelId = @ModelId", bm, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Master", "Edit", VCRHNO: bm.MCODE);
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse { status = "error", result = ex.Message };
            }
        }

        public FunctionResponse DeleteJobLevel(JobLevel jl)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        conn.Execute("DELETE FROM JobLevel WHERE LevelId = @levelid ", jl, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "JobLevel", "Delete", VCRHNO: jl.levelid);
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse { status = "error", result = ex.Message };
            }
        }

        public FunctionResponse UpdateJobLevel(JobLevel jl)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        conn.Execute("UPDATE JobLevel SET Amount = @amount, Stamp = @stamp WHERE LevelId = @levelid ", jl, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "JobLevel", "Edit", VCRHNO: jl.levelid);
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse { status = "error", result = ex.Message };
            }
        }

        public FunctionResponse SaveJobLevel(JobLevel jl)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        conn.Execute("INSERT INTO JobLevel(LevelId, Amount, Stamp) VALUES (@levelid, @amount, @stamp)", jl, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "JobLevel", "New", VCRHNO: jl.levelid);
                        tran.Commit();
                        return new FunctionResponse { status = "ok" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FunctionResponse { status = "error", result = ex.Message };
            }
        }

        public bool SaveBrandModels(SqlTransaction Tran)
        {
            Tran.Connection.Execute("DELETE FROM MultiBrandModel WHERE MCODE = '" + product.MCODE + "'", transaction: Tran);
            if (BMList != null && BMList.Count > 0)
            {   
                if (BMList.Count > 0)
                {
                    foreach (BrandModel bm in BMList)
                    {
                        bm.MCODE = product.MCODE;
                        if (!SaveBrandModel(bm, Tran))
                            return false;
                    }
                }
            }
            return true;
        }
        public bool SaveBrandModel(BrandModel bm, SqlTransaction tran)
        {
            //INSERT INTO MultiBrandModel(MCODE, BrandId, ModelId) VALUES (@MCODE, @BrandId, @ModelId)            
            return tran.Connection.Execute("INSERT INTO MultiBrandModel(MCODE, BrandId, ModelId) SELECT @MCODE, BrandId, @ModelId FROM Model WHERE ModelId = @ModelId", bm, tran) == 1;
        }

        public Task<FunctionResponse> GetMultiBrandModeByMCode(string mCODE)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    var mbm = conn.Query<BrandModel>(@"SELECT MCODE, B.BrandId, B.BrandName, M.ModelId, M.ModelName FROM MultiBrandModel MBM 
                                                        JOIN Brand B ON MBM.BrandId = B.BrandId
                                                        JOIN Model M ON MBM.ModelId = M.ModelId WHERE MCODE = @MCODE", new { MCODE = mCODE });
                    return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "ok", result = mbm });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "error", result = ex.Message });
            }
        }
        #endregion


        public void GetMCode(SqlTransaction tran)
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

        public void GetMenuCode(SqlTransaction tran)
        {
            try
            {
                if (GlobalSetting.GblManualCode == 0)
                {
                    if (product.TYPE == "A")
                    {
                        product.ECODE = Convert.ToDecimal(tran.Connection.ExecuteScalar("SELECT ISNULL(MAX(ECODE),0) + 1 AS EC FROM MENUITEM WHERE FCODE = '" + product.FCODE + "' AND TYPE = 'A'", transaction: tran));
                        product.MENUCODE = product.FCODE.ToString() + "." + product.ECODE.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetMenuCode()
        {
            try
            {
                if (GlobalSetting.GblManualCode == 1 || ManualMcode)
                    return;
                using (SqlConnection Conn = new System.Data.SqlClient.SqlConnection(ConnStr))
                {
                    Conn.Open();
                    using (SqlTransaction tran = Conn.BeginTransaction())
                    {
                        GetMenuCode(tran);
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckDuplicateMenuCode(SqlTransaction tran = null)
        {
            if (tran == null)
                using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnStr))
                    return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MenuItem WHERE MenuCode = @MenuCode AND MCODE <> @MCODE", new { MenuCode = product.MENUCODE, MCODE = product.MCODE }) == 0;
            return tran.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM MenuItem WHERE MenuCode = @MenuCode AND MCODE <> @MCODE", new { MenuCode = product.MENUCODE, MCODE = product.MCODE }, tran) == 0;
        }
        public bool CheckDuplicateMGroupCode()
        {
            if (product.TYPE == "G" && product.LEVELS == 1)
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    return conn.ExecuteScalar<int>(string.Format("SELECT COUNT(*) FROM MENUITEM WHERE FCODE = {0} AND LEVELS = 1", product.FCODE)) == 0;
                }
            }
            return true;
        }

        public bool CheckDuplicateName()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                if (product.TYPE == "G")
                {
                    if (GlobalSetting.GblDuplicateGroup == 1)
                        return true;
                    else if (GlobalSetting.GblDuplicateGroup == 0)
                        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MenuItem WHERE DESCA = @DESCA AND MCODE <> @MCODE", new { DESCA = product.DESCA, MCODE = product.MCODE }) == 0;
                    else if (GlobalSetting.GblDuplicateGroup == 2)
                        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MenuItem WHERE DESCA = @DESCA AND PARENT = @PARENT AND MCODE <> @MCODE", new { DESCA = product.DESCA, MCODE = product.MCODE, PARENT = product.PARENT }) == 0;
                    else
                        return false;
                }
                else
                {
                    if (GlobalSetting.GblDuplicateItem == 1)
                        return true;
                    else if (GlobalSetting.GblDuplicateItem == 0)
                        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MenuItem WHERE DESCA = @DESCA AND MCODE <> @MCODE", new { DESCA = product.DESCA, MCODE = product.MCODE }) == 0;
                    else if (GlobalSetting.GblDuplicateItem == 2)
                        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MenuItem WHERE DESCA = @DESCA AND PARENT = @PARENT AND MCODE <> @MCODE", new { DESCA = product.DESCA, MCODE = product.MCODE, PARENT = product.PARENT }) == 0;
                    else
                        return false;
                }
            }
        }

        public Task<FunctionResponse> GetProductByMCode(string MCODE)
        {
            try
            {
                using (SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConnStr))
                {
                    product = conn.Query<Product>(string.Format(@"SELECT  BARCODE, BASEUNIT, BRAND, CRATE, CRDATE, DESCA, DESCB, DISAMOUNT, DISCONTINUE, DISMODE, DISRATE, ECODE, EDATE, FCODE, HASSERVICECHARGE, ISBARITEM, ISUNKNOWN, LEVELS,
		                                                    MARGIN, MAXLEVEL, MAXSQTY, MAXWARN, MCAT, MCAT1, MCODE, MENUCODE, MGROUP, MINLEVEL, MINWARN, MODEL, PAC, PARENT, PRAC, PRATE_A, 
		                                                    PRATE_B, PRERATE, PRESRATE, PTYPE, RATE_A, RATE_B, RATE_C, RECRATE, REQEXPDATE, ROLEVEL, ROWARN, SAC, SRAC, SUPCODE, SUPITEMCODE, TAXGROUP_ID, 
		                                                    TYPE, VAT, WHOUSE, ZEROROLEVEL FROM MENUITEM WHERE MCODE = '{0}'", MCODE)).First();
                    product.Parent = conn.Query<Product>(string.Format("SELECT MCODE,ISNULL(MENUCODE,'') MENUCODE, ISNULL(PARENT,'') PARENT,DESCA,TYPE, MGROUP, MCAT, MCAT1 FROM MENUITEM WHERE MCODE = '{0}'", product.PARENT)).FirstOrDefault();
                    product.MajorGroup = conn.Query<Product>(string.Format("SELECT MCODE,ISNULL(MENUCODE,'') MENUCODE, ISNULL(PARENT,'') PARENT,DESCA,TYPE, MGROUP FROM MENUITEM WHERE MCODE = '{0}'", product.MGROUP)).FirstOrDefault();
                    product.MultiStockLevels = new ObservableCollection<MultiStockLevel>(conn.Query<MultiStockLevel>(@"SELECT MCODE, WAREHOUSE, ROLEVEL, MINLEVEL, MAXLEVEL FROM MultiStockLevel WHERE MCODE = @MCODE", new { MCODE = MCODE }));
                    if (GlobalSetting.GblEnableRateDiscount == 1)
                        product.ItemRateDiscount = conn.Query<RateDiscount>(@"SELECT MCODE, DTRRATE, WSLRATE, RTLRATE, FLTRATE FROM ItemRateDiscount WHERE MCODE = @MCODE
                                                                        UNION ALL 
                                                                        SELECT @MCODE, 0, 0, 0, 0", new { MCODE = MCODE }).FirstOrDefault();
                    return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "ok", result = product });
                    //
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "error", result = ex.Message });
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
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    IEnumerable<Product> ExsitingMCodeList = conn.Query<Product>("SELECT MCODE, TYPE FROM MENUITEM WHERE MCODE IN (" + InStr + ")");
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
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
        }

        public FunctionResponse GetExistingItemType(IList<int> PTypeList)
        {
            try
            {
                string InStr = PTypeList[0].ToString(); ;
                for (int i = 1; i < PTypeList.Count; i++)
                {
                    InStr += ", " + PTypeList[i];
                }
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    IEnumerable<int> ExsitingPTypeList = conn.Query<int>("SELECT PTYPEID FROM PTYPE WHERE PTYPEID IN (" + InStr + ")");
                    return new FunctionResponse { status = "ok", result = ExsitingPTypeList };
                }
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
        }

        public FunctionResponse GetExistingBrand(IList<int> BrandList)
        {
            try
            {
                string InStr = BrandList[0].ToString();
                for (int i = 1; i < BrandList.Count; i++)
                {
                    InStr += ", " + BrandList[i];
                }
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    IEnumerable<int> ExsitingBrandList = conn.Query<int>("SELECT BrandId FROM Brand WHERE BrandId IN (" + InStr + ")");
                    return new FunctionResponse { status = "ok", result = ExsitingBrandList };
                }
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
        }

        public FunctionResponse GetExistingLevel(IList<string> LevelList)
        {
            try
            {
                string InStr = "'" + LevelList[0].ToString() + "'";
                for (int i = 1; i < LevelList.Count; i++)
                {
                    InStr += ", '" + LevelList[i] + "'";
                }
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    IEnumerable<string> ExsitingBrandList = conn.Query<string>("SELECT LevelId FROM JobLevel WHERE LevelId IN (" + InStr + ")");
                    return new FunctionResponse { status = "ok", result = ExsitingBrandList };
                }
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
        }

        public FunctionResponse GetExistingModel(IList<int> ModelList)
        {
            try
            {
                string InStr = ModelList[0].ToString();
                for (int i = 1; i < ModelList.Count; i++)
                {
                    InStr += ", " + ModelList[i];
                }
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    IEnumerable<int> ExsitingModelList = conn.Query<int>("SELECT ModelId FROM Model WHERE ModelId IN (" + InStr + ")");
                    return new FunctionResponse { status = "ok", result = ExsitingModelList };
                }
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
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
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    IEnumerable<Product> ParentList = conn.Query<Product>("SELECT MCODE, DESCA, MENUCODE, MCAT FROM MENUITEM WHERE MCODE IN (" + InStr + ")");
                    return new FunctionResponse { status = "ok", result = ParentList.ToList() };
                }
            }
            catch (Exception Ex)
            {
                return new FunctionResponse { status = "error", result = Ex.Message };
            }
        }

        public FunctionResponse SaveMCat(dynamic mcat)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MCAT WHERE MENUCAT = @MENUCAT", new {MENUCAT = mcat.name }, tran) > 0)
                        {
                            return new FunctionResponse { status = "error", result = "There is already Category with name '" + mcat.name + "'" };
                        }
                        conn.Execute("INSERT INTO MCAT(MENUCAT, PARENT, TYPE) VALUES(@MENUCAT, '','A')", new { MENUCAT = mcat.name }, tran);
                        ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Category", "New", VCRHNO: mcat.name.ToString());
                        tran.Commit();
                    }
                    return new FunctionResponse { status = "ok" };
                }

            }
            catch (Exception ex)
            {
                return new FunctionResponse
                {
                    status = "error",
                    result = ex.Message
                };
            }
        }        

        public FunctionResponse DeletePType(dynamic mcat)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    if (conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MenuItem WHERE MCAT = @MENUCAT", new {MENUCAT = mcat.name}, tran) > 0)
                    {
                        return new FunctionResponse { status = "error", result = string.Format("Category {0} ({1}) is already linked with item. Thus cannot be deleted", mcat.name, mcat.cat_id) };
                    }
                    conn.Execute("DELETE FROM MCAT WHERE MENUCAT = @MENUCAT", new { MENUCAT = mcat.name }, tran);
                    ActivityLog.SetUserActivityLog(TrnUser, tran, Session.SessionId, "Product Category", "Delete", VCRHNO: mcat.name.ToString());
                    tran.Commit();
                }
                return new FunctionResponse { status = "ok" };
            }
        }

        public bool CheckGroupExists()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MENUITEM WHERE MCODE = @MCODE", new { MCODE = product.PARENT }) > 0;
            }
        }

        public bool CheckPTypeExists()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM PTYPE WHERE PTYPEID = @PTYPEID", new { PTYPEID = product.PTYPE }) > 0;
            }
        }

        public bool HasChildren()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MENUITEM WHERE PARENT = @MCODE", product) > 0;
            }
        }

        public bool HasTransaction()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM RMD_TRNPROD WHERE MCODE = @MCODE", product) > 0;
            }
        }
    }
}

namespace IMS_POS_API.Services
{
    public class ItemSaveDataAccessBase
    {
        public Product product;
        private BarCode BCode;
        private IList<BarCode> BCodeList;
        public Task<FunctionResponse> GetProductBarcodeByMCode(string MCODE)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBCONNECTION))
                {
                    var BCodeList = conn.Query<BarCode>("SELECT BCODE, MCODE, UNIT, ISSUENO, EDATE, BCODEID, SUPCODE, BATCHNO, EXPIRY, INVNO, DIV, FYEAR, SRATE, REMARKS FROM BarCode WHERE MCODE='" + MCODE + "'");
                    return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "ok", result = BCodeList });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<FunctionResponse>(new FunctionResponse { status = "error", result = ex.Message });
            }
        }
    }
}
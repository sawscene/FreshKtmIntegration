using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Model
{
    public class BarCode
    {
        public int SN { get; set; }
        public string BCODE { get; set; }
        public string MCODE { get; set; }
        public string UNIT { get; set; }
        public string ISSUENO { get; set; }
        public Nullable<DateTime> EDATE { get; set; }
        public decimal BCODEID { get; set; }
        public string SUPCODE { get; set; }
        public string BATCHNO { get; set; }
        public Nullable<DateTime> EXPIRY { get; set; }
        public string REMARKS { get; set; }
        public string INVNO { get; set; }
        public string DIV { get; set; }
        public string FYEAR { get; set; }
        public decimal SRATE { get; set; }
        public int ISOLD { get; set; }
        public float STAMP { get; set; }
        public int ISACTIVE { get; set; }
        public bool IsDeactive { get; set; }
        public DateTime ROW_VERSION { get; set; }
        public bool MyProperty { get; set; }

        //Non Table Field
        public string DESCA { get; set; }
        public IList<BarcodeDetail> BCodeDetails { get; set; }

    }
    public class BarcodeDetail
    {
        public string COLUMN_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public int CHARACTER_MAXIMUM_LENGTH { get; set; }
        public object VALUE { get; set; }
    }

    }

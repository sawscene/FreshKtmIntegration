using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Model
{
    public class MenuItem
    {
        public int Serial { get; set; }
        public string MCODE { get; set; }
        public string MENUCODE { get; set; }
        public string DESCA { get; set; }
        public string DESCB { get; set; }
        public string PARENT { get; set; }
        public string TYPE { get; set; }
        public string BASEUNIT { get; set; }
        public string ALTUNIT { get; set; }
        public decimal CONFACTOR { get; set; }
        public decimal RATE_A { get; set; }
        public decimal RATE_B { get; set; }
        public decimal PRATE_A { get; set; }
        public decimal PRATE_B { get; set; }
        public bool VAT { get; set; }
        public decimal MINLEVEL { get; set; }
        public decimal MAXLEVEL { get; set; }
        public decimal ROLEVEL { get; set; }
        public byte MINWARN { get; set; }
        public byte MAXWARN { get; set; }
        public byte ROWARN { get; set; }
        public byte LEVELS { get; set; }
        public string BRAND { get; set; }
        public string MODEL { get; set; }
        public string MGROUP { get; set; }
        public decimal FCODE { get; set; }
        public decimal ECODE { get; set; }
        public string DISMODE { get; set; }
        public decimal DISRATE { get; set; }
        public decimal DISAMOUNT { get; set; }
        public decimal RECRATE { get; set; }
        public decimal MARGIN { get; set; }
        public decimal PRERATE { get; set; }
        public decimal PRESRATE { get; set; }
        public byte DISCONTINUE { get; set; }
        public string MODES { get; set; }
        public decimal PRERATE1 { get; set; }
        public decimal PRERATE2 { get; set; }
        public decimal SCHEME_A { get; set; }
        public decimal SCHEME_B { get; set; }
        public decimal SCHEME_C { get; set; }
        public decimal SCHEME_D { get; set; }
        public decimal SCHEME_E { get; set; }
        public byte FLGNEW { get; set; }
        public byte SALESMANID { get; set; }
        public byte TDAILY { get; set; }
        public byte TMONTHLY { get; set; }
        public byte TYEARLY { get; set; }
        public decimal VPRATE { get; set; }
        public decimal VSRATE { get; set; }
        public string PATH { get; set; }
        public byte PTYPE { get; set; }
        public string SUPCODE { get; set; }
        public string LATESTBILL { get; set; }
        public byte ZEROROLEVEL { get; set; }
        public string MCAT { get; set; }
        public string MIDCODE { get; set; }
        public string SAC { get; set; }
        public string SRAC { get; set; }
        public string PAC { get; set; }
        public string PRAC { get; set; }
        public decimal RATE_C { get; set; }
        public decimal CRATE { get; set; }
        public string GENERIC { get; set; }
        public byte ISUNKNOWN { get; set; }
        public Nullable<DateTime> EDATE { get; set; }
        public byte TSTOP { get; set; }
        public string BARCODE { get; set; }
        public byte HASSERIAL { get; set; }
        public byte HASSERVICECHARGE { get; set; }
        public string DIMENSION { get; set; }
        public decimal FOB { get; set; }
        public string COLOR { get; set; }
        public string PACK { get; set; }
        public string PRODTYPE { get; set; }
        public string GWEIGHT { get; set; }
        public string NWEIGHT { get; set; }
        public string CBM { get; set; }
        public byte HASBATCH { get; set; }
        public string SUPITEMCODE { get; set; }
        public Nullable<DateTime> LPDATE { get; set; }
        public Nullable<DateTime> CRDATE { get; set; }
        public int TAXGROUP_ID { get; set; }
        public decimal LEADTIME { get; set; }
        public string TRNUSER { get; set; }
        public byte ISBARITEM { get; set; }
        public string WHOUSE { get; set; }
        public double MAXSQTY { get; set; }
        public byte REQEXPDATE { get; set; }
        public string STAMP { get; set; }
        public string MCAT1 { get; set; }
        public int STATUS { get; set; }
    }
}

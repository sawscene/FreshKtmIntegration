using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Model
{
    public class MULTIALTUNIT
    {
        public string MCODE { get; set; }
        public string ALTUNIT { get; set; }
        public int CONFACTOR { get; set; }
        public int RATE { get; set; }
        public int ISDEFAULT { get; set; }
        public string BRCODE { get; set; }
        public int PRATE { get; set; }
        public int ISDISCONTINUE { get; set; }
        public int ISDEFAULTPRATE { get; set; }
        public float STAMP { get; set; }
        public int SRATE_DISCOUNT { get; set; }
        public int WSRATE_DISCOUNT { get; set; }
        public int ISACTIVE { get; set; }
        public DateTime ROW_VERSION { get; set; }
        public byte ORIGIN_ROW_VERSION { get; set; }
    }       
}

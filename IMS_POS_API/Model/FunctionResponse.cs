using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Models
{
    public class FunctionResponse<T>
    {
        public string Status { get; set; }
        public T Result { get; set; }
        public string Message { get; set; }
        public string RefNo { get; set; }
        public string Location { get; set; }

        public FunctionResponse()
        {
            Status = "error";
            Message = "Response not set";

        }
    }
    public class FunctionResponse
    {
        public string status { get; set; }
        public object result { get; set; }
       // public string RefNo { get; set; }
       // public string Location { get; set; }

    }

    public class FunctionResponseEventArgs : EventArgs
    {
        public FunctionResponse Response;
    }

}

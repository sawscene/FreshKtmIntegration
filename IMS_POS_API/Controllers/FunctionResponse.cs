using ClientApi.Models;

namespace ClientApi.Controllers
{
    internal class FunctionResponse
    {
        public string status { get; set; }
        public object result { get; set; }
        public string message { get; set; }
        
    }
}
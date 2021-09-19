using IMS_POS_API.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Controllers
{
   // [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginDataAccess loginDataAccess;
        private readonly IConfiguration config;
        public LoginController(LoginDataAccess _loginDataAccess, IConfiguration _config)
        {
            loginDataAccess = _loginDataAccess;
            config = _config;

        }
    }
}

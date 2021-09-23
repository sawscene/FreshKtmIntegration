using IMS_POS_API.DAL;
using IMS_POS_API.Model;
using IMS_POS_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(Login user)
        {
            FunctionResponse<userprofile> userprofile= await loginDataAccess.CheckUser(user);
            if(userprofile.Message == "No User")
                return BadRequest(new { status = "error", error = "Invalid username or password" });
            else if (userprofile.Result == null)
                return BadRequest(userprofile);
            var claims = new[]
            {
                      new Claim(ClaimTypes.NameIdentifier, userprofile.Result.UNAME),
                      new Claim(ClaimTypes.Name, userprofile.Result.UNAME),
                     // new Claim(ClaimTypes.SerialNumber, userProfile.result.CompanyPan.ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                   .GetBytes(config.GetSection("AppSettings:Token").Value));


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddMinutes(10),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                status = "ok",
                token = "bearer " + tokenHandler.WriteToken(token),
                UsersInfo = userprofile.Result

            });
        }
    }
}

using IMS_POS_API.DAL;
using IMS_POS_API.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace IMS_POS_API.Helper
{
    public class BasicAuthenticationHandler:AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ILoginDataAccess loginDataAccess;
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,ILoginDataAccess _loginDataAccess)
            : base(options, logger, encoder, clock)
        {
            this.loginDataAccess = _loginDataAccess;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
            userprofile user = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];
                user = await loginDataAccess.CheckUser(username, password);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
            if (user == null)
                return AuthenticateResult.Fail("Invalid Username or Password");
            var claims = new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, user.UNAME.ToString()),
                new Claim(ClaimTypes.Name, user.UNAME),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}

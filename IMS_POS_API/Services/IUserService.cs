using IMS_POS_API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Services
{
    public interface IUserService
    {
        Task<userprofile> CheckUser(string username, string password);
    }
}

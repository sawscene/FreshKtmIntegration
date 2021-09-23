using Dapper;
using IMS_POS_API.Model;
using IMS_POS_API.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS_POS_API.DAL
{
    public class LoginDataAccess:ILoginDataAccess
    {
        string db;
        public LoginDataAccess(IConfiguration config)
        {
            db = config.GetConnectionString("myConnectionString");
        }
        public async Task<userprofile> CheckUser(string username, string password)
        {
            using (SqlConnection con = new SqlConnection(db))
            {
                con.Open();
                var user =await con.QueryAsync($"SELECT UNAME, PASSWORD FROM USERPROFILES WHERE UNAME = @username AND PASSWORD = @password", new { username, password=Encrypt(password)});
                if (user.Any())
                {
                    var conuser = new userprofile() { UNAME = username, PASSWORD = password };
                    return conuser;
                }
                return null;
            }
        }

        public static string Encrypt(string txtValue, string Key = "AmitLalJoshi")
        {
            int i;
            string TextChar;
            string KeyChar;

            string retMsg = "";
            int ind = 1;

            for (i = 1; i <= Convert.ToInt32(txtValue.Length); i++)
            {
                TextChar = txtValue.Substring(i - 1, 1);
                ind = i % Key.Length;
                KeyChar = Key.Substring((ind));
                byte str1 = Encoding.Default.GetBytes(TextChar)[0];
                byte str2 = Encoding.Default.GetBytes(KeyChar)[0];
                var encData = str1 ^ str2;
                retMsg = retMsg + Convert.ToChar(encData).ToString();
            }
            return retMsg;
        }
    }
}

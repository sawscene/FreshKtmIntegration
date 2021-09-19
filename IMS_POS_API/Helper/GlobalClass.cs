using Dapper;
using IMS_POS_API.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.Helper
{
    public class GlobalClass
    {
        string conString;
        // public static string conString = ConnectionModel.ConnectionString;
        public GlobalClass(IConfiguration config)
        {
            this.conString = config.GetConnectionString("myConnectionString");
        }
        public DateTime GetServerDate()
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                var serverDateString = con.Query<DateTime>("select getdate()").FirstOrDefault();

                var serverDate = serverDateString.Date;
                return serverDate;
            }
        }

        public string GetServerTime()
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                var serverDateString = con.Query<DateTime>("select getdate()").FirstOrDefault();

                var serverTime = serverDateString.ToString("HH:mm:ss");
                return serverTime;
                //date.DATE.ToString("hh.mm tt")
            }
        }
        public  DateTime GetServerDateTime()
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                return con.ExecuteScalar<DateTime>("select getdate()");
            }
        }
    }
}

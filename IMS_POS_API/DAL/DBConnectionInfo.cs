using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.DAL
{
    public class DBConnectionInfo
    {
        string Connection = ConfigurationManager.ConnectionStrings["myConnectionString"].ToString();
        
        public SqlConnection GetConnection()
        {
            SqlConnection con = new SqlConnection(Connection);
            return con;
        }
    }
}

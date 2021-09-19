using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace IMS_POS_API.Helper
{
    public class HelperClass
    {
        public static string HMAC_SHA256(string keyString, string message)
        {
            byte[] key = Encoding.ASCII.GetBytes(keyString);
            HMACSHA256 sha = new HMACSHA256(key);
            byte[] input = Encoding.ASCII.GetBytes(message);
            return BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLower();
        }

        public static bool CheckHash(string Key, string Message, string HashValue)
        {
            var has = HMAC_SHA256(Key, Message);
            if (has.Equals(HashValue))
            {
                return true;
            }
            return false;
        }
        public static string UrlEncode(string stringToEncode)
        {
            string lower = HttpUtility.UrlEncode(stringToEncode);
            Regex reg = new Regex(@"%[a-f0-9]{2}");
            return reg.Replace(lower, m => m.Value.ToUpperInvariant());
        }
    }
}

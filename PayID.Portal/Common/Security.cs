using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PayID.Portal.Common
{
    public class Security
    {
        /// <summary>
        /// Tên hàm     : CreateMD5Hash
        /// </summary>
        /// <param name="input">chuỗi cần mã hóa</param>
        /// <returns>kết quả mã hóa</returns>
        private static string CreateMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.Unicode.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string CreatPassWordHash(string pass)
        {
            return CreateMD5Hash(pass);
        }
    }
}
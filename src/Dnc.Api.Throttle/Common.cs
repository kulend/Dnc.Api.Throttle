using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Dnc.Api.Throttle
{
    public static class Common
    {
        public const string HeaderStatusKey = "Api-Throttle-Status";
        public const string GlobalApiKey = "global";

        public static string IpToNum(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return ip;

            IPAddress ipAddr = IPAddress.Parse(ip);
            List<Byte> ipFormat = ipAddr.GetAddressBytes().ToList();
            ipFormat.Reverse();
            ipFormat.Add(0);
            BigInteger ipAsInt = new BigInteger(ipFormat.ToArray());
            return ipAsInt.ToString();
        }

        public static string EncryptMD5(string value)
        {
            var str = "";
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                str = sBuilder.ToString();
            }
            return str.ToLower();
        }

        public static string EncryptMD5Short(string value)
        {
            return EncryptMD5(value).Substring(8, 16);
        }
    }
}

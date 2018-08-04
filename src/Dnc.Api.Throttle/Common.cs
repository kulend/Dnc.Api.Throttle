using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;

namespace Dnc.Api.Throttle
{
    public static class Common
    {
        public const string HeaderStatusKey = "DApi-Throttle-Status";
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
    }
}

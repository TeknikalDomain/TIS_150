using System;

namespace TIS_150
{
    class B64
    {
        public static string Encode(byte[] rawData)
        {
            return Convert.ToBase64String(rawData).Replace('/', '_');
        }
    }
}
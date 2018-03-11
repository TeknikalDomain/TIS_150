using System.Security.Cryptography;

namespace TIS_150
{
    class Hash
    {
        public static byte[] Gen(byte[] data)
        {
            SHA256Managed hashGen = new SHA256Managed();
            return hashGen.ComputeHash(data);
        }
    }
}

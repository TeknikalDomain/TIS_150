using System;
using System.Collections.Generic;
using System.IO;

namespace TIS_150
{
    internal class RC4
    {
        private static byte[] s;
        private static int i;
        private static int j;
        private static int kl;

        public static void Schedule(string key)
        {
            kl = key.Length;
            s = new byte[256];
            for (i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
            }

            j = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % kl]) % 256;
                byte temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }
        }

        public static void CipherFile(string filepath)
        {
            byte[] fdata;
            List<byte> efdata = new List<byte>();
            byte k;
            i = 0;
            j = 0;
            fdata = File.ReadAllBytes(filepath);
            foreach (byte fbyte in fdata)
            {
                i = (i + 1) % 256;
                j = (j + s[i]) % 256;
                byte temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                k = s[(s[i] + s[j]) % 256];
                efdata.Add((byte)(fbyte ^ k));
            }
            using (BinaryWriter writer = new BinaryWriter(File.Open(filepath, FileMode.Create)))
            {
                writer.Write(efdata.ToArray());
            }
        }
    }
}

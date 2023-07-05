




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsMatrix.Core
{
    public static class AssetMatrixStaticFunction
    {

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string ExtractStringFromPath(string s)
        {
            string extractedStringAsset = TrimString(s);
            string sRemove = ExtractString_filetype(extractedStringAsset);

            return sRemove;
        }

        public static string ExtractString_filetype(string s)
        {
            int idx = s.IndexOf('.');
            string sRemove = s.Remove(idx);

            return sRemove;
        }

        public static string TrimString(string s)
        {
            string sTrim = ReverseString(s);
            if (s == null) return null;
            int idx = sTrim.IndexOf('/');
            string sRemove = sTrim.Remove(idx);
            string sResult = ReverseString(sRemove);

            return sResult;
        }

        public static string ReverseString(string s)
        {
            if (s == null) return null;
            char[] charS = s.ToCharArray();
            Array.Reverse(charS);
            return new string(charS);
        }
    }
}

using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static class HashHelper
    {
        public static string GetHash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var data = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();

                foreach(byte b in data)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool CompareHash(string input, string compareToHash)
        {
            string inHash = GetHash(input);
            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(inHash, compareToHash) == 0;
        }
    }
}
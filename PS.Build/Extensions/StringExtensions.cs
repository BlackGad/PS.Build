using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PS.Build.Extensions
{
    public static class StringExtensions
    {
        #region Static members

        public static string GetMD5Hash(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            using (MD5 md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                return hashBytes.Aggregate(string.Empty, (agg, b) => agg + b.ToString("X2"));
            }
        }

        public static int Occurrences(this string input, char value)
        {
            var count = 0;
            for (var index = 0; index < input.Length; index++)
            {
                if (input[index] == value) count++;
            }
            return count;
        }

        #endregion
    }
}
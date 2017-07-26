using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace PS.Build.Essentials.Extensions
{
    static class SecureStringExtensions
    {
        #region Static members

        public static SecureString ToSecureString(this string unsecureString)
        {
            if (unsecureString == null) return null;

            return unsecureString.Aggregate(new SecureString(), AppendChar, MakeReadOnly);
        }

        public static string ToUnsecureString(this SecureString secureString)
        {
            if (secureString == null) throw new ArgumentNullException("secureString");

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private static SecureString AppendChar(SecureString ss, char c)
        {
            ss.AppendChar(c);
            return ss;
        }

        private static SecureString MakeReadOnly(SecureString ss)
        {
            ss.MakeReadOnly();
            return ss;
        }

        #endregion
    }
}
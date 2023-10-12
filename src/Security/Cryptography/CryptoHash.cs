namespace Radical.Security.Cryptography
{
    using Radical.Extracting;
    using System.Security.Cryptography;
    using System.Text;
    using Radical.Uniques;

    public static class CryptoHash
    {
        private static readonly KeyedHashAlgorithm ALGORITHM = new HMACSHA512();

        public static string Encrypt(string pass, string salt)
        {
            byte[] bIn = pass.GetBytes();
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bRet = null;

            KeyedHashAlgorithm kha = ALGORITHM;

            if (kha.Key.Length == bSalt.Length)
            {
                kha.Key = bSalt;
            }
            else if (kha.Key.Length < bSalt.Length)
            {
                byte[] bKey = new byte[kha.Key.Length];
                bKey.CopyBlock(bSalt, (uint)bKey.Length);
                kha.Key = bKey;
            }
            else
            {
                byte[] bKey = new byte[kha.Key.Length];
                for (int iter = 0; iter < bKey.Length; )
                {
                    int len = Math.Min(bSalt.Length, bKey.Length - iter);
                    bKey.CopyBlock(bSalt, (uint)iter, (uint)bKey.Length);
                    iter += len;
                }

                kha.Key = bKey;
            }

            bRet = kha.ComputeHash(bIn);

            return Convert.ToBase64String(bRet);
        }

        public static string Salt()
        {
            return Convert.ToBase64String(Unique.NewKey.GetBytes());
        }

        public static bool Verify(string hashedPassword, string hashedSalt, string providedPassword)
        {
            string salt = hashedSalt;
            if (
                String.Equals(
                    Encrypt(providedPassword, salt),
                    hashedPassword,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
                return true;
            else
                return false;
        }
    }
}

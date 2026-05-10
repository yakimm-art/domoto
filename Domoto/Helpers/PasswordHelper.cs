using System;
using System.Security.Cryptography;
using System.Text;

namespace Domoto.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltBytes  = 16;
        private const int HashBytes  = 32;
        private const int Iterations = 100_000;

        // Returns a string like: $pbkdf2-sha256$100000$<base64-salt>$<base64-hash>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltBytes];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = Pbkdf2(password, salt, Iterations, HashBytes);

            return string.Format("$pbkdf2-sha256${0}${1}${2}",
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        // Returns true if the stored hash was made with the old SHA-256 scheme
        // and should be upgraded on next successful login.
        public static bool NeedsRehash(string stored)
        {
            return IsLegacyHash(stored);
        }

        public static bool VerifyPassword(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            // Legacy: 64-char hex SHA-256 — still accepted, triggers rehash
            if (IsLegacyHash(stored))
                return LegacySha256(password) == stored;

            // New format: $pbkdf2-sha256$<iter>$<salt>$<hash>
            // Split gives ["", "pbkdf2-sha256", iter, salt, hash]
            var parts = stored.Split('$');
            if (parts.Length != 5 || parts[1] != "pbkdf2-sha256")
                return false;

            int iterations;
            if (!int.TryParse(parts[2], out iterations)) return false;

            byte[] salt;
            byte[] expected;
            try
            {
                salt     = Convert.FromBase64String(parts[3]);
                expected = Convert.FromBase64String(parts[4]);
            }
            catch { return false; }

            byte[] actual = Pbkdf2(password, salt, iterations, expected.Length);
            return FixedTimeEquals(expected, actual);
        }

        // ---- Private helpers ----

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int bytes)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, salt, iterations))
                return kdf.GetBytes(bytes);
        }

        private static bool IsLegacyHash(string stored)
        {
            if (stored == null || stored.Length != 64) return false;
            foreach (char c in stored)
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            return true;
        }

        private static string LegacySha256(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder(64);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // Constant-time comparison to prevent timing attacks
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}

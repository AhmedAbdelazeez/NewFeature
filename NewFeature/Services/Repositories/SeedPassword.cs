using System;
using System.Security.Cryptography;

namespace NewFeature.Services.Repositories
{
    // Initial password for an account the startup seeders have to create because it is missing.
    // It used to be the same hard-coded "Rawahil@123" for every account, which meant anyone who had
    // read the source could sign in as any department (and the admin password was also reset back to
    // it on every restart). Each account now gets its own random password, printed once to the
    // console so whoever started the server can hand it over and change it afterwards.
    public static class SeedPassword
    {
        // No look-alike characters (I/l/1, O/0) and no quotes, spaces or backslashes, so the value
        // survives being copied out of a console, a chat message or a spreadsheet.
        private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Lower = "abcdefghijkmnpqrstuvwxyz";
        private const string Digits = "23456789";
        private const string Symbols = "!@#$%^&*-_=+?";

        public static string Generate(int length = 18)
        {
            var all = Upper + Lower + Digits + Symbols;

            // At least two of each class, the rest drawn from the full set, then shuffled so the
            // guaranteed characters don't always sit at the front.
            var chars = new char[length];
            int i = 0;
            foreach (var set in new[] { Upper, Lower, Digits, Symbols })
            {
                chars[i++] = set[RandomNumberGenerator.GetInt32(set.Length)];
                chars[i++] = set[RandomNumberGenerator.GetInt32(set.Length)];
            }
            while (i < length) chars[i++] = all[RandomNumberGenerator.GetInt32(all.Length)];

            for (int n = chars.Length - 1; n > 0; n--)
            {
                int k = RandomNumberGenerator.GetInt32(n + 1);
                (chars[n], chars[k]) = (chars[k], chars[n]);
            }
            return new string(chars);
        }
    }
}

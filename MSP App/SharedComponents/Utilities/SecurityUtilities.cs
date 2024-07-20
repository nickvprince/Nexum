using System.Security.Cryptography;
using System.Text;

namespace SharedComponents.Utilities
{
    public class SecurityUtilities
    {
        public static string? Shuffle(string? key1, string? key2)
        {
            if (string.IsNullOrEmpty(key1) || string.IsNullOrEmpty(key2))
            {
                return null;
            }
            var result = new StringBuilder(32);
            int maxLength = Math.Max(key1.Length, key2.Length);
            for (int i = 0; i < maxLength; i++)
            {
                if (result.Length < 32 && i < key1.Length)
                {
                    result.Append(key1[i]);
                }
                if (result.Length < 32 && i < key2.Length)
                {
                    result.Append(key2[i]);
                }
            }
            // If the result is shorter than 32 characters, repeat the keys to fill up to 32 characters
            int resultLength = result.Length;
            int key1Index = 0;
            int key2Index = 0;
            while (result.Length < 32)
            {
                if (resultLength + key1Index < key1.Length && result.Length < 32)
                {
                    result.Append(key1[key1Index]);
                    key1Index++;
                }
                if (resultLength + key2Index < key2.Length && result.Length < 32)
                {
                    result.Append(key2[key2Index]);
                    key2Index++;
                }
            }
            return result.ToString();
        }
        public static string? Encrypt(string? key, string? input)
        {
            if(string.IsNullOrEmpty(key) || string.IsNullOrEmpty(input))
            {
                return null;
            }
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(input);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string? Decrypt(string? key, string input)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(input))
            {
                return null;
            }
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB; // Using ECB mode as it does not require an IV
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(input)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static string? GenerateDefaultRandomPassword()
        {
            string password = "Nexum";
            Random random = new Random();
            for (int i = 0; i < 3; i++)
            {
                password += (int)random.Next(0, 9);
            }
            password += "!";
            return password;
        }

        public static string? PadKey(string key, int length)
        {
            if (key.Length >= length)
            {
                return key.Substring(0, length);
            }
            return key.PadRight(length, '0');
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Vault.Core
{
    public static class FilePasswordEncryption
    {
        public static async Task<long> Encrypt(Stream inputStream, Stream outputStream, string userId, string password)
        {
            try
            {
                var totalLength = 0L;
                var sha256 = SHA256.Create();
                using (var aes = RijndaelManaged.Create())
                {
                    aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    aes.IV = GetIvFrom(userId);

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                        {
                            var buffer = new byte[2048];
                            var length = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                            totalLength += length;
                            do
                            {
                                await csEncrypt.WriteAsync(buffer, 0, length);
                                length = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                                totalLength += length;
                            } while (length > 0);
                        }
                    }
                }
                return totalLength + ((totalLength % 16) == 0 ? 0 : (16 - (totalLength % 16)));
            }
            catch (CryptographicException e)
            {
                throw e;
            }
        }

        public static async Task<bool> Decrypt(Stream inputStream, Stream outputStream, string userId, string password)
        {
            try
            {
                var sha256 = SHA256.Create();
                using (var aes = new RijndaelManaged())
                {
                    aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    aes.IV = GetIvFrom(userId);

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                        {
                            var buffer = new byte[2048];
                            var length = await csEncrypt.ReadAsync(buffer, 0, buffer.Length);
                            do
                            {
                                await outputStream.WriteAsync(buffer, 0, length);
                                length = await csEncrypt.ReadAsync(buffer, 0, buffer.Length);
                            } while (length > 0);
                        }
                    }
                }
                return true;
            }
            catch (CryptographicException e)
            {
                throw e;
            }


        }

        public static byte[] GetIvFrom(string value)
        {
            var sha256 = SHA256.Create();
            var fullSha = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            byte[] buffer = new byte[16];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = fullSha[i];
            }
            return buffer;
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Vault.Core
{
    public static class SecureDelete
    {
        public static async Task<bool> Delete(string filePath)
        {
            try
            {
                var length = new FileInfo(filePath).Length;
                var currentPosition = 0l;

                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                {
                    var buffer = new byte[2048];
                    var bufferLength = buffer.Length;
                    while (currentPosition < length)
                    {
                        if((length - currentPosition) < bufferLength)
                        {
                            bufferLength = (int) (length - currentPosition);
                        }
                        var generator = RandomNumberGenerator.Create();
                        generator.GetBytes(buffer, 0, bufferLength);

                        await file.WriteAsync(buffer, 0, bufferLength);
                        currentPosition += bufferLength;
                    }
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static async Task<bool> Delete(string filePath, int pass)
        {
            if (pass < 1) throw new ArgumentOutOfRangeException("pass must be more than 0.");

            try
            {
                Console.WriteLine($"Secure Delete started with {pass} pass");

                for (int i = 0; i < pass; i++)
                {
                    Console.WriteLine($"Overwriting data {i + 1} stage...");
                    await Delete(filePath);
                }

                Console.WriteLine($"Deleting actual source file ...");
                File.Delete(filePath);

                Console.WriteLine($"Secure delete is in completed.");
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

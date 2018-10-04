using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Vault.Core;

namespace Vault.UX.ConsoleUx
{
    class Program
    {
        static void Main(string[] args)
        {

            //RijndaelExample.AMain();
            var buffer = new byte[1024 * 1024];
            var length = 0L;
            string outputFilepath = Path.Combine(Environment.CurrentDirectory, "important-Encrypted.txt");
            if (File.Exists(outputFilepath)) File.Delete(outputFilepath);
            using (var encryptedDataStream = new MemoryStream(buffer, true))//new FileStream(outputFilepath, FileMode.CreateNew, FileAccess.Write))//
            {
                encryptedDataStream.Seek(0, SeekOrigin.Begin);

                using (var fileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, "importantfile.txt"), FileMode.Open, FileAccess.Read))
                {
                    length = FilePasswordEncryption.Encrypt(fileStream, encryptedDataStream, "KARAN", "SUMAN").GetAwaiter().GetResult();
                }
            }

            Console.WriteLine("Encrypted data:");
            //Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, (int)length));
            Console.WriteLine($"Data length {buffer.Length}");

            using (var encryptedDataStream = new MemoryStream(buffer, 0, (int)length))
            {
                encryptedDataStream.Seek(0, SeekOrigin.Begin);

                if (File.Exists("decrypted.txt")) File.Delete("decrypted.txt");
                using (var memory = new MemoryStream())//new FileStream("decrypted.txt", FileMode.CreateNew, FileAccess.Write))
                {
                    FilePasswordEncryption.Decrypt(encryptedDataStream, memory, "KARAN", "SUMAN").GetAwaiter().GetResult();

                    memory.Seek(0, SeekOrigin.Begin);

                    Console.WriteLine("Decrypted data:");
                    var allText = memory.ToArray();
                    Console.WriteLine(Encoding.UTF8.GetString(allText));
                    Console.WriteLine($"Data length {allText.Length}");
                }
            }




            //string outputFilepath = Path.Combine(Environment.CurrentDirectory, "important-Encrypted.txt");

            //using (var fileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, "importantfile.txt"), FileMode.Open, FileAccess.Read))
            //{

            //    if (File.Exists(outputFilepath)) File.Delete(outputFilepath);
            //    using (var outFileStream = new FileStream(outputFilepath, FileMode.CreateNew, FileAccess.Write))
            //    {
            //        FilePasswordEncryption.Encrypt(fileStream, outFileStream, "KARAN", "SUMAN").GetAwaiter().GetResult();
            //    }
            //}

            //using (var inputFile = new FileStream(outputFilepath, FileMode.Open, FileAccess.Read))
            //{
            //    using (var memory = new MemoryStream())
            //    {
            //        FilePasswordEncryption.Decrypt(inputFile, memory, "KARAN", "SUMAN").GetAwaiter().GetResult();

            //        memory.Seek(0, SeekOrigin.Begin);
            //        var allText = memory.ToArray();
            //        Console.WriteLine(Encoding.UTF8.GetString(allText));
            //        Console.WriteLine($"Data length {allText.Length}");
            //    }
            //}
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        class RijndaelExample
        {
            public static void AMain()
            {
                try
                {

                    string original = "Here is some data to encrypt!";

                    // Create a new instance of the RijndaelManaged
                    // class.  This generates a new key and initialization 
                    // vector (IV).
                    using (RijndaelManaged myRijndael = new RijndaelManaged())
                    {

                        myRijndael.GenerateKey();
                        myRijndael.GenerateIV();
                        // Encrypt the string to an array of bytes.
                        byte[] encrypted = EncryptStringToBytes(original, myRijndael.Key, myRijndael.IV);

                        // Decrypt the bytes to a string.
                        string roundtrip = DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

                        //Display the original data and the decrypted data.
                        Console.WriteLine("Original:   {0}", original);
                        Console.WriteLine("Round Trip: {0}", roundtrip);

                        Console.WriteLine("Original:   {0}", encrypted.Length);
                        Console.WriteLine("Round Trip: {0}", roundtrip);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
            }
            static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
            {
                // Check arguments.
                if (plainText == null || plainText.Length <= 0)
                    throw new ArgumentNullException("plainText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("IV");
                byte[] encrypted;
                // Create an RijndaelManaged object
                // with the specified key and IV.
                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;

                    // Create an encryptor to perform the stream transform.
                    ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {

                                //Write all data to the stream.
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }


                // Return the encrypted bytes from the memory stream.
                return encrypted;

            }

            static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
            {
                // Check arguments.
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("IV");

                // Declare the string used to hold
                // the decrypted text.
                string plaintext = null;

                // Create an RijndaelManaged object
                // with the specified key and IV.
                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                }

                return plaintext;

            }
        }
    }
}

﻿using Consoles.Infrastructure;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vault.Core;

namespace Vault.UX.ConsoleUx
{
    class Program
    {
        static void Main(string[] args)
        {
            var interpreter = new CommandInterpreter(new CommandInterpreterOptions { KeepCommandIdentifier = true });
            var commandData = interpreter.InterpretTokenBase(args);
            var pipeline = new ApplicationPipline();
            var isInInteractiveMode = false;

            pipeline
                .Add("--help", "-h", (_, next) => {
                    Console.WriteLine("You are in help. We will not run anything else in the pipeline.");
                    return Task.CompletedTask;
                })
                .Add(new DelegateCommand("--read", (file, next) => {
                    if (File.Exists(file))
                        Console.WriteLine(File.ReadAllText(file));
                    else
                        Console.WriteLine($"\"{file}\" is not found");
                    return next();
                }))
                .Add(new DelegateCommand("--read-delay", (file, next) => {
                    if (File.Exists(file))
                    {
                        Console.WriteLine($"Reading file at \"{file}\"...");
                        var lines = File.ReadLines(file);
                        foreach (var item in lines)
                        {
                            Console.WriteLine(item);
                            Task.Delay(100).Wait();
                        }
                        Console.WriteLine("EOF reached");
                    }
                    else
                        Console.WriteLine($"\"{file}\" is not found");
                    return next();
                }))
                .Add(new DelegateCommand("--print", "-p", (message, next) => {
                    Console.WriteLine(message);
                    return next();
                }))
                .Add("--it", (_, next) => {
                    isInInteractiveMode = true;
                    return next();
                });
            pipeline.Execute(commandData);

            string input = null;
            while (isInInteractiveMode && input != "q")
            {
                Console.Write("$ ");
                input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    args = $"--{input}".Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    commandData = interpreter.InterpretTokenBase(args);
                    pipeline.Execute(commandData);
                }
            }
        }

        static void SimpleDemo()
        {
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

        }
    }
}

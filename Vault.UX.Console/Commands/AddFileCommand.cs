using Consoles.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Vault.Core;

namespace Vault.UX.ConsoleUx.Commands
{
    public class AddFileCommand : Command
    {
        public AddFileCommand(string commandNamespace = null) : base("--AddFile", "-add", commandNamespace)
        {
        }

        public override Task Execute(InterpreterReadToken token, Func<Task> next)
        {
            if (Global.InputFiles == null)
                Global.InputFiles = new List<string>() { token.Value };
            else
                Global.InputFiles.Add(token.Value);

            Console.WriteLine(token.Value);
            return next();
        }
    }

    public class ExecuteBatchCommand : Command
    {
        public ExecuteBatchCommand(string commandNamespace = null) : base("--Execute", "-e", commandNamespace)
        {
        }

        public override Task Execute(InterpreterReadToken token, Func<Task> next)
        {
            if (!Global.WorkLoad.HasValue)
            {
                Console.WriteLine("Work load type is not set. Use SetWorkLoad to set work load.");
                return Task.CompletedTask;
            }

            switch (Global.WorkLoad.Value)
            {
                case WorkLoad.SingleFile:
                    if(Global.WorkType.Value == WorkType.Encrypt)
                        return EncryptSingleFile();
                    else
                        return DecryptSingleFile();
                default:
                    break;
            }

            return Task.CompletedTask;
        }

        protected virtual async Task EncryptSingleFile()
        {
            var output = string.IsNullOrEmpty(Global.OutputFile) ? Global.InputFiles[0] + ".out.encrypt" : Global.OutputFile;
            var password = Input.GetPassword("Password: ", '*', 32);


            using (var encryptedDataStream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
            {
                using (var fileStream = new FileStream(Global.InputFiles[0], FileMode.Open, FileAccess.Read))
                {
                    var length = await FilePasswordEncryption.Encrypt(fileStream, encryptedDataStream, Global.UserId, password);
                    Console.WriteLine($"File Encrypted with {length} length");
                }
            }
        }

        protected virtual async Task DecryptSingleFile()
        {
            var output = string.IsNullOrEmpty(Global.OutputFile) ? Global.InputFiles[0] + ".out.decrypt" : Global.OutputFile;
            var password = Input.GetPassword("Password: ", '*', 32);


            using (var decryptedDataStream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
            {
                using (var fileStream = new FileStream(Global.InputFiles[0], FileMode.Open, FileAccess.Read))
                {
                    var result = await FilePasswordEncryption.Decrypt(fileStream, decryptedDataStream, Global.UserId, password);
                    if(result)
                        Console.WriteLine($"File decrypted successfully.");
                    else
                        Console.WriteLine($"Failed to decrypted file. Reason may include,\nWrong User name or password.");
                }
            }
        }
    }
}

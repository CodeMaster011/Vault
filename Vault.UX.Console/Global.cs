using System.Collections.Generic;

namespace Vault.UX.ConsoleUx
{
    public static class Global
    {
        public static List<string> InputFiles { get; set; } = new List<string>();
        public static string OutputFile { get; set; }
        public static WorkLoad? WorkLoad { get; set; }
        public static WorkType? WorkType { get; set; } = ConsoleUx.WorkType.Encrypt;
        public static string Password { get; set; }
        public static string UserId { get; set; } = "suman";
    }

    public enum WorkType
    {
        Encrypt,
        Decrypt
    }

    public enum WorkLoad
    {
        SingleFile
    }
}

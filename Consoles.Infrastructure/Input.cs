using System;

namespace Consoles.Infrastructure
{
    public static class Input
    {
        public static string GetPassword(string message, char screenChar, int maxLength)
        {
            var data = new char[maxLength];
            var length = 0;

            Console.Write(message);
            var input = Console.ReadKey(true);
            while (input.Key != ConsoleKey.Enter)
            {
                if(input.Key == ConsoleKey.Backspace)
                {
                    if(length > 0)
                    {
                        Console.CursorLeft--;
                        Console.Write(" ");
                        Console.CursorLeft--;
                        length--;
                    }
                    
                }
                else
                {
                    Console.Write(screenChar);
                    if(length >= maxLength)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Max length reached");
                        break;
                    }
                    else
                    {
                        data[length++] = input.KeyChar;
                    }
                }

                input = Console.ReadKey(true);
            }
            Console.WriteLine();
            return new string(data, 0, length);
        }
    }
}

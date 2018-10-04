using System;

namespace Consoles.Infrastructure
{
    public class Progress
    {
        public static void Report(int progress, int total) {
            drawTextProgressBar(progress, total);
        }

        public static void Report(int progress, string progressText) {
            drawTextProgress(progress, progressText);
        }

        public static void Report(string progress, string progressText) {
            drawTextProgress(progress, progressText);
        }

        private static void drawTextProgress(int progress, string progressText) =>
            drawTextProgress(progress.ToString(), progressText);
        private static void drawTextProgress(string progress, string progressText) {
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.CursorLeft = 3;
            Console.Write(progress);
            Console.Write(" ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.CursorLeft = 13;
            Console.Write(progressText);
            Console.Write(" ");

            Console.ForegroundColor = ConsoleColor.Black;
            Console.CursorLeft = Console.WindowWidth;
            Console.CursorVisible = true;
        }
        private static void drawTextProgressBar(int progress, int total)
        {
            Console.CursorVisible = false;

            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31 ; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess

            Console.CursorVisible = true;
        }
    }
}
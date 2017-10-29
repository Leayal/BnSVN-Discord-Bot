using System;

namespace BnSVN_Discord_Bot
{
    class Program
    {
        private static Bot botInstance;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            botInstance = new Bot();
            botInstance.Run(args);

            // Friendly Blocking exit ???
            ConsoleKeyInfo keyinfo = Console.ReadKey(false);
            while (keyinfo.Key != ConsoleKey.Escape)
                keyinfo = Console.ReadKey(false);

            botInstance.Stop(5000);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            botInstance.Stop(5000);
            Environment.Exit(0);
        }
    }
}

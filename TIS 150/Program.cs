using System;
using System.IO;

namespace TIS_150
{
    public class Program
    {
        internal static IDB currentIDB;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // No dir specified, use .
                DirectoryInfo currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());

                Console.WriteLine("No directory specified, assuming CWD. ({0}\\)\n", currentDir.Name);
                try
                {
                    currentIDB = new IDB(currentDir);
                    CCP.Start();
                    return;
                }
                catch (IDBInvalidException e)
                {
                    Console.Error.WriteLine("ERROR: DB Invalid: {0}", e.Message);
                    return;
                }
            }
            else
            {
                // Directory should have been specified as first (and only) argument, check validity.
                // UNLESS that is -c

                if (args[0].Equals("-c"))
                {
                    if (args.Length == 2)
                    {
                        IDB.Init(args[1]);
                        return;
                    }
                    else
                    {
                        IDB.Init(Directory.GetCurrentDirectory());
                        return;
                    }
                }
                if (Directory.Exists(args[0]))
                {
                    try
                    {
                        currentIDB = new IDB(new DirectoryInfo(args[0]));
                    }
                    catch (IDBInvalidException e)
                    {
                        Console.Error.WriteLine("ERROR: DB Invalid: {0}", e.Message);
                        return;
                    }
                    CCP.Start();
                    return;
                }
                else
                {
                    Console.Error.WriteLine("ERROR: Invalid directory specified: {0}", args[0]);
                }
            }
        }

        public static void ClearLine()
        {
            int len = Console.CursorLeft;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', len));
            Console.CursorLeft = 0;
        }
    }
}

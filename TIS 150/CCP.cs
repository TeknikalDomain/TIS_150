using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace TIS_150
{
    class CCP
    {
        public static void Start()
        {
            Console.WriteLine("\n\nTIS 150 v1.0");
            Console.WriteLine("Written by Teknikal_Domain");
            Console.WriteLine(@"Use ""help"" or ""?."" to see commands.");
            Console.WriteLine("All commands end with a .\n");

            bool running = true;
            string rawinput;
            string[] rawWords;
            string[] words;
            while (running)
            {
                rawinput = null;
                rawWords = null;
                words = null;
                Console.Write("TIS> "); rawinput = Console.ReadLine();
                if (rawinput.Equals("")) continue;
                while (rawinput.Last() != '.')
                {
                    Console.Write("  -> "); rawinput = rawinput + " " + Console.ReadLine();
                }
            if (Directory.Exists(Program.currentIDB.Dir.FullName + "/.temp")) { Directory.Delete(Program.currentIDB.Dir.FullName + "/.temp", true); /* Thread.Sleep(2000); */}
                string input = rawinput.ToUpper().Trim().TrimEnd('.');
                rawWords = rawinput.Trim().TrimEnd('.').Split(' ');
                words = input.Split(' ');
                if (words.Length == 1 && !(words[0].Equals("EXIT") || words[0].Equals("SAVE") || words[0].Equals("HELP") || words[0].Equals("CLS") || words[0].Equals("?")))
                {
                    Console.WriteLine("Error: No argument provided.");
                    continue;
                }
                switch (words[0])
                {
                    case "EXIT":
                        Shutdown();
                        return;

                    case "?":
                    case "HELP":
                        ShowHelp();
                        break;

                    case "CLS":
                    case "CLEAR":
                        Console.Clear();
                        break;

                    case "SEARCH":
                        if (words[1].Equals("ALL"))
                        {
                            ISAM.Display(ISAM.SearchAll(words.Skip(2).ToArray()));
                        }
                        else
                        {
                            ISAM.Display(ISAM.Search(words.Skip(1).ToArray()));
                        }
                        break;

                    case "VIEW":
                        switch (words[1])
                        {
                            case "US":
                                ISAM.DisplayUnsorted();
                                break;

                            case "DB":
                                long dsize = Program.currentIDB.GetSize();
                                if (dsize >= 1024)
                                {
                                    dsize /= 1024; // B to KB
                                    if (dsize >= 1024)
                                    {
                                        double ddsize = dsize / 1024; // kB to MB
                                        if (ddsize >= 1024)
                                        {
                                            ddsize /= 1024; // MB to GB
                                            Math.Round(ddsize, 2);
                                            Console.WriteLine("Database size: {0} GB", ddsize);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Database size: {0} MB", ddsize);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Database size: {0} kB", dsize);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Database size: {0} Bytes", dsize);
                                }
                                Console.WriteLine("Item count: {0}", Program.currentIDB.Sorted.Count);
                                Console.WriteLine("Unsorted item count: {0}", Program.currentIDB.Unsorted.Count);
                                break;

                            case "TAGS":
                            ISAM.DisplayTags();
                                break;

                            case "ALL":
                                ISAM.DisplayAll();
                                break;

                            default:
                                if (rawWords[1].Length == 44)
                                {
                                    ISAM.Display(rawWords[1]);
                                }
                                else
                                {
                                    if (rawWords[1].Length > 44)
                                    {
                                        Console.WriteLine("Error: Invalid IID.");
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error: must specify complete IID.");
                                        break;
                                    }
                                }
                                break;
                        }
                        break;

                    case "SAVE":
                        Program.currentIDB.Dump();
                        break;

                    case "EDIT":
                        switch (words[1])
                        {
                            case "US":
                                ISAM.ClassifyUnsorted();
                                break;

                            case "DB":
                                Console.WriteLine("Database obfuscation: {0}", Program.currentIDB.Obfuscation ? "yes" : "no");
                                Console.Write("{0} database obfuscation? ", Program.currentIDB.Obfuscation ? "Disable" : "Enable"); string choice = Console.ReadLine();
                                if (choice.ToUpper().Equals("Y"))
                                {
                                    Program.currentIDB.Obfuscation = !Program.currentIDB.Obfuscation;
                                    if (Program.currentIDB.Obfuscation)
                                    {
                                        Console.Write("Enter obfuscation key: "); Program.currentIDB.Key = Console.ReadLine();
                                    }
                                    else
                                    {
                                        File.Delete(Program.currentIDB.Dir.FullName + "/.obfuscate");
                                        File.Delete(Program.currentIDB.Dir.FullName + "/.integrity");
                                    }
                                    Console.WriteLine("Database obfuscation {0}", Program.currentIDB.Obfuscation ? "enabled." : "disabled.");
                                }
                                break;
                            default:
                                if (rawWords[1].Length == 44)
                                {
                                    ISAM.Edit(rawWords[1]);
                                }
                                else
                                {
                                    if (rawWords[1].Length > 44)
                                    {
                                        Console.WriteLine("Error: Invalid IID.");
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error: must specify complete IID.");
                                        break;
                                    }
                                }
                                break;
                        }
                        break;

                    case "ADD":
                        ISAM.Add(rawWords[1]);
                        break;

                    case "DELETE":
                        switch (words[1])
                        {
                            case "US":
                                ISAM.Clean();
                                break;

                            case "ALL":
                                ISAM.Wipe();
                                break;

                            default:
                                ISAM.Delete(ISAM.Search(rawWords.Skip(1).ToArray()));
                                break;
                        }
                        break;

                    default:
                        Console.WriteLine("Error: unrecognized command.");
                        break;
                }
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("All commands are terminated with a .");
            Console.WriteLine("Simple breakdown:\n");
            Console.WriteLine("EXIT.\tExits the program.");
            Console.WriteLine("HELP. or ?.\tDisplay this message");
            Console.WriteLine("SEARCH <TAG>\tSearch the database and return all results that contain a tag.");
            Console.WriteLine("VIEW <IID|US|DB|ALL>\tEither view a specific image, or information about the database.");
            Console.WriteLine("EDIT <IID|US|DB>\tEdit image tags, classify unsorted images, and change settings.");
            Console.WriteLine("SAVE TO <DLOC>\tSave database images to the specified directory.");
            Console.WriteLine("ADD <FLOC|DLOC>\tAdd either individual images or an entire folder of images to the database. folders will be unsorted.");
            Console.WriteLine("DELETE <TAG|US|ALL>\tDelete an individual image, images with a tag, everything unsorted, or everything. Be careful.\n");
            Console.WriteLine("Argument list:");
            Console.WriteLine("IID: Image identifier. base64 encoded sha256 hash. Must be a complete (44 char) string.");
            Console.WriteLine("TAG: User-defined text for sorting. Usually follows the format group:tag. prepend a - to negate results.");
            Console.WriteLine("US: All unsorted images. (images without tags)");
            Console.WriteLine("DB: Indicates the databse, used to VIEW and EDIT settings.");
            Console.WriteLine("ALL: Every SORTED image, US excluded.");
            Console.WriteLine("FLOC: Path to a FILE.");
            Console.WriteLine("DLOC: path to a directory.\n");
        }

        private static void Shutdown()
        {
            Program.currentIDB.Save();
        }
    }
}

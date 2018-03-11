using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIS_150
{
    internal class IDB
    {
        private DirectoryInfo DBDir;
        private Dictionary<string, List<string>> sortedData;
        private List<string> unsortedData;
        private bool ob = false;
        private string dbkey;
        private const string integrate = "TIS v1.0 Integrity check.\n\nDO NOT CHANGE OR REMOVE THIS FILE!!!";

        public DirectoryInfo Dir
        {
            get
            {
                return DBDir;
            }
        }
        public Dictionary<string, List<string>> Sorted
        {
            get
            {
                return sortedData;
            }
        }
        public List<string> Unsorted
        {
            get
            {
                return unsortedData;
            }
        }

        public bool Obfuscation
        {
            get
            {
                return ob;
            }
            set
            {
                ob = value;
            }
        }

        public string Key
        {
            get
            {
                return dbkey;
            }
            set
            {
                dbkey = value;
            }
        }

        public IDB(DirectoryInfo currentDir)
        {
            ConsoleColor dcol = Console.ForegroundColor;
            DBDir = currentDir;

            if (Directory.Exists(DBDir.FullName + @"/.temp")) Directory.Delete(DBDir.FullName + @"/.temp", true);

            Console.WriteLine("---Validating DB structure---");

            Console.Write("[WORK] .US folder exists");
            if (Directory.Exists(DBDir.FullName + "/.US"))
            {
                Program.ClearLine();
                Console.Write("["); Console.ForegroundColor = ConsoleColor.Green; Console.Write("PASS"); Console.ForegroundColor = dcol; Console.Write("]  .US folder exists\n");
            }
            else
            {
                Program.ClearLine();
                Console.Write("["); Console.ForegroundColor = ConsoleColor.Red; Console.Write("FAIL"); Console.ForegroundColor = dcol; Console.Write("]  .US folder exists\n");
                Console.WriteLine("Verification failed. Unable to open DB.");
                throw new IDBInvalidException(".US folder either does not exist or cannot be accessed.");
            }

            Console.Write("[WORK] checking folder naming (2 b64 digits)");
            Regex dirNaming = new Regex("^[A-Za-z0-9+_][A-Za-z0-9+_]$");
            foreach (DirectoryInfo dir in DBDir.EnumerateDirectories())
            {
                if (dir.Name.Equals(".US"))
                {
                    continue;
                }

                if (dir.Name.Length == 2 && !dirNaming.Match(dir.Name).Success)
                {
                    Program.ClearLine();
                    Console.Write("["); Console.ForegroundColor = ConsoleColor.Red; Console.Write("FAIL"); Console.ForegroundColor = dcol; Console.Write("]  checking folder naming (2 b64 digits)\n");
                    Console.WriteLine("Verification failed. Unable to open DB.");
                    throw new IDBInvalidException("Extraneous folders, or incorrect naming scheme.");
                }
            }
            Program.ClearLine();
            Console.Write("["); Console.ForegroundColor = ConsoleColor.Green; Console.Write("PASS"); Console.ForegroundColor = dcol; Console.Write("]  checking folder naming (2 b64 digits)\n");

            Console.Write("[WORK] .DBTags file exists");
            if (File.Exists(DBDir.FullName + "/.DBTags"))
            {
                Program.ClearLine();
                Console.Write("["); Console.ForegroundColor = ConsoleColor.Green; Console.Write("PASS"); Console.ForegroundColor = dcol; Console.Write("]  .DBTags file exists\n");
            }
            else
            {
                Program.ClearLine();
                Console.Write("["); Console.ForegroundColor = ConsoleColor.Red; Console.Write("FAIL"); Console.ForegroundColor = dcol; Console.Write("]  .DBTags file exists\n");
                Console.WriteLine("Verification failed. Unable to open DB.");
                throw new IDBInvalidException("Cannot locate .DBTags file.");
            }

            if (File.Exists(DBDir.FullName + "/.obfuscate"))
            {
                ob = true;
                Console.WriteLine("\nObfuscation marker detected. Checking integrity.");
                Console.Write("[WORK] .integrity file exists");
                if (File.Exists(DBDir.FullName + "/.integrity"))
                {
                    Program.ClearLine();
                    Console.Write("["); Console.ForegroundColor = ConsoleColor.Green; Console.Write("PASS"); Console.ForegroundColor = dcol; Console.Write("]  .integrity file exists\n");
                    Console.Write("Key required: "); dbkey = Console.ReadLine();
                    Console.Write("[WORK] validating integrity");
                    File.Copy(DBDir.FullName + "/.integrity", DBDir.FullName + "/.itemp");
                    RC4.Schedule(dbkey);
                    RC4.CipherFile(DBDir.FullName + "/.integrity");
                    using (StreamReader reader = new StreamReader(new FileStream(DBDir.FullName + "/.integrity", FileMode.Open)))
                    {
                        string checkstring = reader.ReadToEnd();
                        if (checkstring.Equals(integrate))
                        {
                            File.Delete(DBDir.FullName + "/.itemp");
                            Program.ClearLine();
                            Console.Write("["); Console.ForegroundColor = ConsoleColor.Green; Console.Write("PASS"); Console.ForegroundColor = dcol; Console.Write("]  validating integrity\n");
                        }
                        else
                        {
                            File.Delete(DBDir.FullName + "/.integrity");
                            File.Move(DBDir.FullName + "/.itemp", DBDir.FullName + "/.integrity");
                            Program.ClearLine();
                            Console.Write("["); Console.ForegroundColor = ConsoleColor.Red; Console.Write("FAIL"); Console.ForegroundColor = dcol; Console.Write("]  validating integrity\n");
                            Console.WriteLine("Verification failed. Unable to open DB.");
                            throw new IDBInvalidException(".integrity file needed for obfuscated database.");
                        }
                    }
                    RC4.Schedule(dbkey);
                    RC4.CipherFile(DBDir.FullName + "/.DBTags");
                    foreach (DirectoryInfo dir in DBDir.EnumerateDirectories())
                    {
                        if (dir.Name.Equals(".US")) continue;
                        foreach (FileInfo file in dir.GetFiles())
                        {
                            Console.Write("Deobfuscating {0}/{1}...", dir.Name, file.Name);
                            RC4.Schedule(dbkey);
                            RC4.CipherFile(file.FullName);
                            Program.ClearLine();
                        }
                    }
                }
                else
                {
                    Program.ClearLine();
                    Console.Write("["); Console.ForegroundColor = ConsoleColor.Red; Console.Write("FAIL"); Console.ForegroundColor = dcol; Console.Write("]  .integrity file exists\n");
                    Console.WriteLine("Verification failed. Unable to open DB.");
                    throw new IDBInvalidException(".integrity file does not match, integrity unknown.");

                }
            }

            Console.WriteLine("\nVerification complete. Reading database...");
            sortedData = new Dictionary<string, List<string>>();
            unsortedData = new List<string>();
            Console.Write("Image count: 0");
            int binCount = 0;
            foreach (DirectoryInfo dir in DBDir.EnumerateDirectories())
            {
                if (dir.Name.Equals(".US"))
                {
                    continue;
                }
                binCount++;
                foreach (FileInfo file in dir.EnumerateFiles())
                {
                    sortedData.Add(dir.Name + file.Name.Split('.')[0], null);
                    Program.ClearLine();
                    Console.Write("Image count: {0}", sortedData.Count);
                }
            }
            Console.Write("\nUnsorted image count: 0");
            foreach (FileInfo file in new DirectoryInfo(DBDir.FullName + @"/.US/").EnumerateFiles())
            {
                unsortedData.Add(file.Name);
                Program.ClearLine();
                Console.Write("Unsorted image count: {0}", unsortedData.Count);
            }
            double binFill;
            double ic = (double)sortedData.Count;
            binFill = ic / (double)binCount;
            Console.WriteLine();
            Console.Write("Bin count: {0} [{1}]", binCount, binFill);
            Console.WriteLine();
            Console.Write("Reading tag list: 0 [0]");
            using (StreamReader dbt = new StreamReader(DBDir.FullName + @"/.DBTags"))
            {
                string currentItem = "";
                int items = 0;
                int tags = 0;
                while (!dbt.EndOfStream)
                {
                    string line = dbt.ReadLine().Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    if (line[0].Equals('\\'))
                    {
                        currentItem = line.Split('\\')[1];
                        sortedData[currentItem] = new List<string>();
                        items++;
                    }
                    else
                    {
                        sortedData[currentItem].Add(line.ToUpper());
                        tags++;
                    }
                    Program.ClearLine();
                    Console.Write("Reading tag list: {0} [{1}]", tags, items);
                }
            }
        }

        public static void Init(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!Directory.EnumerateFileSystemEntries(directory).Any())
            {
                DirectoryInfo dbi = new DirectoryInfo(directory);
                dbi.CreateSubdirectory(".US");
                File.Create(dbi.FullName + "/.DBTags").Close();
            }
            else
            {
                throw new IDBInvalidException("Directory not empty.");
            }
        }

        public void Dump()
        {
            DirectoryInfo dumpDir = Directory.CreateDirectory(DBDir.Parent.FullName + "/TISDUMP");
            foreach (DirectoryInfo dir in DBDir.EnumerateDirectories())
            {
                if (dir.Name.Equals(".US"))
                {
                    dumpDir.CreateSubdirectory("Unsorted");
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        Console.Write("Saving unsorted {0}...", file.Name);
                        File.Copy(file.FullName, dumpDir.FullName + "/" + file.Name);
                        Program.ClearLine();
                    }
                }
                else
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        Console.Write("Saving {0}/{1}...", dir.Name, file.Name);
                        File.Copy(file.FullName, dumpDir.FullName + "/" + dir.Name + file.Name);
                        Program.ClearLine();
                    }
                }
            }
        }

        public void Save()
        {
            UpdateDBTags();
            if (ob)
            {

                File.Create(DBDir.FullName + "/.obfuscate").Dispose();
                using (StreamWriter writer = new StreamWriter(new FileStream(DBDir.FullName + "/.integrity", FileMode.Create)))
                {
                    writer.Write(integrate);
                }

                foreach (FileInfo file in DBDir.GetFiles())
                {
                    if (file.Name.Equals(".obfuscate")) continue;
                    if (file.Name.Equals(".dbtags.pl")) continue;
                    RC4.Schedule(dbkey);
                    RC4.CipherFile(file.FullName);
                }

                foreach (DirectoryInfo dir in DBDir.EnumerateDirectories())
                {
                    if (dir.Name.Equals(".US")) continue;
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        Console.Write("Obfuscating {0}/{1}...", dir.Name, file.Name);
                        RC4.Schedule(dbkey);
                        RC4.CipherFile(file.FullName);
                        Program.ClearLine();
                    }
                }
            }
            sortedData = null;
            unsortedData = null;
        }

        public void UpdateDBTags()
        {
            using (StreamWriter dbt = new StreamWriter(DBDir.FullName + @"/.DBTags"))
            {
                for (int i = 0; i < sortedData.Count; i++)
                {
                    dbt.WriteLine("\\" + sortedData.ElementAt(i).Key);
                    sortedData.ElementAt(i).Value.Sort();
                    foreach (string tag in sortedData.ElementAt(i).Value.ToArray())
                    {
                        dbt.WriteLine(tag.ToUpper());
                    }
                    dbt.WriteLine();
                }
                dbt.Flush();
            }
            using (StreamWriter prodb = new StreamWriter(new FileStream(DBDir.FullName + "/.dbtags.pl", FileMode.Create)))
            {
                List<string> Tags = new List<string>();
                for (int i = 0; i < sortedData.Count; i++)
                {
                    foreach (string tag in sortedData.ElementAt(i).Value.ToArray())
                    {
                        prodb.WriteLine("tagged('" + sortedData.ElementAt(i).Key + "', '" + tag.ToUpper() + "').");
                    }
                }

                prodb.WriteLine();

                for (int i = 0; i <sortedData.Count; i++)
                {
                    foreach (string tag in sortedData.ElementAt(i).Value.ToArray())
                    {
                        if (!Tags.Contains(tag))
                        {
                            prodb.WriteLine("group('" + tag.Split(':')[0].ToUpper() + "', '" + tag.Split(':')[1].ToUpper() + "').");
                            Tags.Add(tag);
                        }
                    }

                }
                prodb.Flush();
            }

        }

        public long GetSize()
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = DBDir.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = DBDir.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetSize(di);
            }
            return size;
        }

        private long GetSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetSize(di);
            }
            return size;
        }

    }
}

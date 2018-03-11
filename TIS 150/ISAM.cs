using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TIS_150
{
    class ISAM
    {
        public static void Display(string[] iids)
        {
            if (iids.Count() == 0)
            {
                Console.WriteLine("No results to display.");
                return;
            }
            Directory.CreateDirectory(Program.currentIDB.Dir.FullName + @"/.temp");
            DirectoryInfo tempDir = new DirectoryInfo(Program.currentIDB.Dir.FullName + @"/.temp");
            foreach (string iid in iids)
            {
                string dir = iid.Substring(0, 2);
                string file = iid.Substring(2);
                DirectoryInfo searchDir = new DirectoryInfo(Program.currentIDB.Dir.FullName + "/" + dir);
                foreach (FileInfo item in searchDir.EnumerateFiles())
                {
                    if (item.Name.Split('.')[0].Equals(file))
                    {
                        File.Copy(item.FullName, tempDir.FullName + "/" + dir + item.Name);
                    }
                }
            }
            Process.Start(tempDir.FullName);
        }

        public static void Display(string iid)
        {
            bool found = false;

            string dir = iid.Substring(0, 2);
            string file = iid.Substring(2);
            DirectoryInfo searchDir = new DirectoryInfo(Program.currentIDB.Dir.FullName + "/" + dir);
            foreach (FileInfo item in searchDir.EnumerateFiles())
            {
                if (item.Name.Split('.')[0].Equals(file))
                {
                    found = true;
                    Process.Start(item.FullName);
                    break;
                }
            }
            if (!found)
            {
                Console.WriteLine("No results to display.");
            }
        }

        public static void DisplayUnsorted()
        {
            if (Program.currentIDB.Unsorted.Count == 0)
            {
                Console.WriteLine("No results to display.");
                return;
            }
            Directory.CreateDirectory(Program.currentIDB.Dir.FullName + @"/.temp");
            foreach (string item in Program.currentIDB.Unsorted.ToArray())
            {
                File.Copy(Program.currentIDB.Dir.FullName + @"/.US/" + item, Program.currentIDB.Dir.FullName + @"/.temp/" + item);
            }
            Process.Start(Program.currentIDB.Dir.FullName + @"/.temp/");
        }

        public static void DisplayTags()
        {
            Stopwatch tk = new Stopwatch();
            tk.Start();
            int itemCount = Program.currentIDB.Sorted.Count;
            int tagCount = 0;
            int catCount = 0;
            List<string> tagList = new List<string>();
            List<string> catList = new List<string>();
            Dictionary<string, int> catPop = new Dictionary<string, int>();
            for (int i = 0; i < itemCount; i++)
            {
                string[] itemTags = Program.currentIDB.Sorted.ElementAt(i).Value.ToArray();
                foreach (string tag in itemTags)
                {
                    string cat = tag.Split(':')[0];
                    if (!catList.Contains(cat))
                    {
                        catList.Add(cat);
                        catCount++;
                        catPop.Add(cat, 0);
                    }
                    if (!tagList.Contains(tag))
                    {
                        tagList.Add(tag);
                        tagCount++;
                        catPop[cat] = catPop[cat] + 1;
                    }
                }
            }
            tagList.Sort();
            tk.Stop();
            double millis = (double)tk.ElapsedMilliseconds;
            double em = Math.Round(millis / 1000.0, 2);
            tagList.Sort();
            foreach (string tag in tagList.ToArray())
            {
                Console.WriteLine(tag);
            }
            Console.Write("\n---Press any key to continue---"); Console.ReadKey(); Program.ClearLine();
            Console.WriteLine("CATEGORIES: ");
            foreach (string cat in catList.ToArray())
            {
                Console.Write("{0}: {1} [{2}%]\n", cat, catPop[cat], Math.Round(catPop[cat] / (float)tagCount, 2));
            }
            Console.WriteLine("\n{1} tags found, {2} categories. ({0} secs)", em, tagCount, catCount);

        }

        public static void DisplayAll()
        {
            Display(Program.currentIDB.Sorted.Keys.ToArray());
        }

        public static string[] Search(string[] searchTags)
        {
            Stopwatch tk = new Stopwatch();
            tk.Start();
            int itemCount = Program.currentIDB.Sorted.Count;
            List<string> itags = new List<string>();
            List<string> etags = new List<string>();
            List<string> results = new List<string>();

            foreach (string tag in searchTags)
            {
                if (tag.First() == '-')
                {
                    etags.Add(tag.TrimStart('-'));
                }
                else
                {
                    itags.Add(tag);
                }
            }

            for (int i = 0; i < itemCount; i++)
            {
                bool excluded = false;
                string[] itemTags = Program.currentIDB.Sorted.ElementAt(i).Value.ToArray();
                foreach (string itemTag in itemTags)
                {
                    if (etags.Contains(itemTag))
                    {
                        excluded = true;
                        break;
                    }
                }
                if (!excluded)
                {
                    foreach (string itemTag in itemTags)
                    {
                        if (itags.Contains(itemTag))
                        {
                            results.Add(Program.currentIDB.Sorted.ElementAt(i).Key);
                            break;
                        }
                    }
                }
            }
            tk.Stop();
            double millis = (double)tk.ElapsedMilliseconds;
            double em = Math.Round(millis / 1000.0, 2);
            Console.WriteLine("{0} retults in set. ({1} secs)", results.Count, em);
            return results.ToArray();
        }

        public static string[] SearchAll(string[] searchTags)
        {
            Stopwatch tk = new Stopwatch();
            tk.Start();
            int itemCount = Program.currentIDB.Sorted.Count;
            List<string> itags = new List<string>();
            List<string> etags = new List<string>();
            List<string> results = new List<string>();

            foreach (string tag in searchTags)
            {
                if (tag.First() == '-')
                {
                    etags.Add(tag.TrimStart('-'));
                }
                else
                {
                    itags.Add(tag);
                }
            }

            for (int i = 0; i < itemCount; i++)
            {
                bool excluded = false;
                string[] itemTags = Program.currentIDB.Sorted.ElementAt(i).Value.ToArray();
                foreach (string itemTag in itemTags)
                {
                    if (etags.Contains(itemTag))
                    {
                        excluded = true;
                        break;
                    }
                }

                foreach (string itag in itags)
                {
                    if (!itemTags.Contains(itag))
                    {
                        excluded = true;
                        break;
                    }
                }
                if (!excluded)
                {
                    results.Add(Program.currentIDB.Sorted.ElementAt(i).Key);
                }
            }
            tk.Stop();
            double millis = (double)tk.ElapsedMilliseconds;
            double em = Math.Round(millis / 1000.0, 2);
            Console.WriteLine("{0} retults in set. ({1} secs)", results.Count, em);
            return results.ToArray();
        }

        public static void Edit(string iid)
        {
            bool found = false;

            string dir = iid.Substring(0, 2);
            string file = iid.Substring(2);
            DirectoryInfo searchDir = new DirectoryInfo(Program.currentIDB.Dir.FullName + "/" + dir);
            foreach (FileInfo item in searchDir.EnumerateFiles())
            {
                if (item.Name.Split('.')[0].Equals(file))
                {
                    found = true;
                    Process.Start(item.FullName);
                    break;
                }
            }
            if (found)
            {
                int itemcount = Program.currentIDB.Sorted.Count;
                int itemindex = -1;

                for (int i = 0; i < itemcount; i++)
                {
                    if (Program.currentIDB.Sorted.ElementAt(i).Key.Equals(iid))
                    {
                        itemindex = i;
                        break;
                    }
                }
                if (itemindex == -1)
                {
                    Console.WriteLine("Error: No image with specified IID in database.");
                    return;
                }
                List<string> itemTags = new List<string>(Program.currentIDB.Sorted.ElementAt(itemindex).Value);
                bool editloop = true;
                while (editloop)
                {
                    for (int t = 0; t < itemTags.ToArray().Length; t++)
                    {
                        Console.WriteLine("{0}", itemTags.ElementAt(t));
                    }
                    Console.WriteLine("\n0) Save and exit");
                    Console.WriteLine("1) Add tag");
                    Console.WriteLine("2) Remove tag\n");
                    Console.Write("Pleae enter a selection: "); string selection = Console.ReadLine();
                    int snum;
                    if (int.TryParse(selection, out snum))
                    {
                        switch (snum)
                        {
                            case 0:
                                editloop = false;
                                string key = Program.currentIDB.Sorted.ElementAt(itemindex).Key;
                                Program.currentIDB.Sorted[key] = itemTags;
                                break;

                            case 1:
                                Console.Write("Enter tag to add: "); string tag = Console.ReadLine().Trim().ToUpper();
                                if (tag.Equals(""))
                                {
                                    Console.WriteLine("No tag entered. Aborting.");
                                    break;
                                }
                                itemTags.Add(tag);
                                break;

                            case 2:
                                Console.Write("Enter tag to remove: "); tag = Console.ReadLine().Trim().ToUpper();
                                if (tag.Equals(""))
                                {
                                    Console.WriteLine("No tag entered. Aborting.");
                                    break;
                                }
                                if (!itemTags.Remove(tag))
                                {
                                    Console.WriteLine("Tag not associated. Ignoring.");
                                }
                                break;

                            default:
                                Console.WriteLine("Error: Invalid selection");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid selection");
                        continue;
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: No image with specified IID.");
            }
        }

        public static void ClassifyUnsorted()
        {
            if (Program.currentIDB.Unsorted.Count == 0)
            {
                Console.WriteLine("No unsorted images in database.");
                return;
            }
            Process.Start(Program.currentIDB.Dir.FullName + @"/.US");
            bool classify = true;
            while (classify)
            {
                if (Program.currentIDB.Unsorted.Count == 0)
                {
                    Console.WriteLine("No unsorted images in database.");
                    return;
                }
                for (int i = 0; i < Program.currentIDB.Unsorted.Count; i++)
                {
                    Console.WriteLine("{0}) {1}", i, Program.currentIDB.Unsorted.ElementAt(i));
                }
                Console.Write("\nPlease enter a selection (-1 to quit): "); string selection = Console.ReadLine().Trim();
                int snum;
                if (int.TryParse(selection, out snum))
                {
                    switch (snum)
                    {
                        case -1:
                            classify = false;
                            break;

                        default:
                            if (snum > Program.currentIDB.Unsorted.Count)
                            {
                                Console.WriteLine("Error: Invalid selection.");
                            }
                            else
                            {
                                AddUS(Program.currentIDB.Unsorted.ElementAt(snum));
                            }
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Error: Invalid selection.");
                    continue;
                }
            }
        }

        public static void Add(string rawfile)
        {
            if (Directory.Exists(rawfile))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(rawfile);
                Console.WriteLine("Adding {0} items to unsorted.", dirinfo.GetFiles().Length);
                foreach (FileInfo file in dirinfo.EnumerateFiles())
                {
                    Console.WriteLine("Adding file {0}...", file.Name);
                    Program.currentIDB.Unsorted.Add(file.Name);
                    File.Move(file.FullName, Program.currentIDB.Dir.FullName + @"/.US/" + file.Name);
                }
            }
            else
            {
                if (File.Exists(rawfile))
                {
                    FileInfo file = new FileInfo(rawfile);
                    Process.Start(file.FullName);
                    Console.Write("Enter tags, separated by space. Nothing to add to unsorted: "); string rawtags = Console.ReadLine().Trim().ToUpper();
                    if (rawtags.Equals(""))
                    {
                        Console.WriteLine("No tags specified, moving to unsorted.");
                        File.Move(file.FullName, Program.currentIDB.Dir.FullName + @"/.US/" + file.Name);
                        Program.currentIDB.Unsorted.Add(file.Name);
                        return;
                    }
                    List<string> tags = new List<string>(rawtags.Split(' '));
                    FileStream fstream = file.Open(FileMode.Open);
                    byte[] fdata = new byte[fstream.Length];
                    fstream.Read(fdata, 0, (int)fstream.Length);
                    fstream.Dispose();
                    string ihash = B64.Encode(Hash.Gen(fdata));
                    string dbdir = ihash.Substring(0, 2);
                    string dbfile = ihash.Substring(2);
                    if (!Directory.Exists(Program.currentIDB.Dir.FullName + "/" + dbdir))
                    {
                        Program.currentIDB.Dir.CreateSubdirectory(dbdir);
                    }
                    File.Move(file.FullName, Program.currentIDB.Dir.FullName + "/" + dbdir + "/" + dbfile + "." + file.Name.Split('.')[1]);
                    Program.currentIDB.Sorted.Add(ihash, tags);
                    Program.currentIDB.UpdateDBTags();
                }
                else
                {
                    Console.WriteLine("Error: Invalid file or directory.");
                }
            }
        }

        public static void AddUS(string USImg)
        {
            Program.currentIDB.Unsorted.Remove(USImg);
            Add(Program.currentIDB.Dir.FullName + @"/.US/" + USImg);
        }

        public static void Save(string dir)
        {
            DirectoryInfo di = new DirectoryInfo(dir);
            DirectoryInfo sloc = di.CreateSubdirectory(Program.currentIDB.Dir.Name);
            DirectoryInfo usloc = sloc.CreateSubdirectory(".US");
            Console.WriteLine("Saving unsorted images...");
            foreach (string usname in Program.currentIDB.Unsorted.ToArray())
            {
                Console.WriteLine("Saving {0}", usname);
                File.Copy(Program.currentIDB.Dir.FullName + @"/.US/" + usname, usloc.FullName + "/" + usname);
            }
            Console.WriteLine("\nSaving sorted images...");
            int itemCount = Program.currentIDB.Sorted.Count;
            for (int i = 0; i < itemCount; i++)
            {
                string rawname = Program.currentIDB.Sorted.ElementAt(i).Key;
                string dirname = rawname.Substring(0, 2);
                string fname = rawname.Substring(2);

                DirectoryInfo tdinfo = new DirectoryInfo(Program.currentIDB.Dir.FullName + "/" + dirname);
                foreach (FileInfo file in tdinfo.EnumerateFiles())
                {
                    if (file.Name.Split('.')[1].Equals(fname))
                    {
                        Console.WriteLine("Saving {0}", rawname);
                        File.Copy(file.FullName, sloc.FullName + "/" + dirname + "/" + file.FullName);
                    }
                }
            }

        }

        public static void Clean()
        {
            Console.Write("Really delete all unsorted images? (enter \"YES\", case sensitive): "); string confirm = Console.ReadLine().Trim();
            if (confirm.Equals("YES"))
            {
                Console.WriteLine("Deleting all unsorted images...");
                DirectoryInfo usdir = new DirectoryInfo(Program.currentIDB.Dir.FullName + @"/.US");
                foreach (FileInfo file in usdir.EnumerateFiles())
                {
                    file.Delete();
                    Program.currentIDB.Unsorted.Remove(file.Name);
                }
            }
            else
            {
                Console.WriteLine("Delete not confirmed. aborting.");
                return;
            }
        }

        public static void Delete(string[] iids)
        {
            if (iids.Count() == 0)
            {
                Console.WriteLine("IID not found.");
                return;
            }
            foreach (string iid in iids)
            {
                string dir = iid.Substring(0, 2);
                string file = iid.Substring(2);
                DirectoryInfo searchDir = new DirectoryInfo(Program.currentIDB.Dir.FullName + "/" + dir);
                foreach (FileInfo item in searchDir.EnumerateFiles())
                {
                    if (item.Name.Split('.')[0].Equals(file))
                    {
                        File.Delete(item.FullName);
                    }
                }
            }

        }

        public static void Wipe()
        {
            Console.Write("Are you REALLY sure? Enter \"Yes, please delete\" to cinform: "); string confirm = Console.ReadLine().Trim();
            if (confirm.Equals("Yes, please delete"))
            {
                DirectoryInfo root = Program.currentIDB.Dir;
                foreach (DirectoryInfo dir in root.EnumerateDirectories())
                {
                    if (dir.Name.Equals(".US"))
                    {
                        continue;
                    }
                    else
                    {
                        Directory.Delete(dir.FullName, true);
                    }
                }
            }
            else
            {
                Console.WriteLine("Delete not confirmed. aborting.");
                return;
            }
        }
    }
}

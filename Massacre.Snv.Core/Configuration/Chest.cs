using System;
using System.IO;
using System.Reflection;

namespace Massacre.Snv.Core.Configuration
{
    public class Chest
    {
        public static Chest Get()
        {
            return new Chest(Assembly.GetCallingAssembly().GetName().Name);
        }

        readonly static char ps = Path.PathSeparator;
        readonly static string ext = ".txt";

        public string Name { get; protected set; }
        public string Storage { get; protected set; }

        public string FilePath
        {
            get { return strg + Name + ext; }
        }

        private string strg
        {
            get
            {
                if (Storage.Length == 0) return Storage;
                return Storage + ps;
            }
        }

        public ChestDictionary<string, string> Data { get; protected set; }

        public Chest(string name, string dir = null)
        {
            if (name == null || name.Trim().Length == 0)
            {
                throw new ArgumentException("Name must not be empty");
            }

            Name = name;
            if (dir == null)
            {
                Storage = "";
            }
            else
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                Storage = dir + ps;
            }

            Data = new ChestDictionary<string, string>(FilePath);

            if (!File.Exists(FilePath))
            {
                string[] names =
                {
                    strg + Name + "Template" + ext,
                    strg + "Template" + ext,
                    "Template" + ext,
                };

                foreach (var n in names)
                {
                    if (File.Exists(n))
                    {
                        File.Copy(n, FilePath);
                        break;
                    }
                }
            }

            if (File.Exists(FilePath))
            {
                LoadFile(FilePath);
            }
        }

        protected void LoadFile(string fn)
        {
            var data = (ChestDictionary<string, string>)Data;
            data.WriteAllowed = false;

            try
            {
                using (var f = new StreamReader(new FileStream(fn, FileMode.Open)))
                {
                    while (true)
                    {
                        var line = f.ReadLine();
                        if (line == null) break;
                        if (!line.Contains(" ")) break;
                        if (line.Trim().Length == 0) break;

                        var split = line.Split(new char[] { ' ' }, 2);
                        if (split.Length < 2)
                        {
                            continue;
                        }

                        Data.Add(split[0], split[1]);
                    }
                }
            }

            catch { }

            data.WriteAllowed = true;
        }
    }
}

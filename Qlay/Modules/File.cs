
using System.Collections.Generic;

namespace Qlay.Modules
{
    class File
    {
        private string patch;

        public File(string patch)
        {
            this.patch = patch;
        }

        public bool Exists(string file)
        {
            return System.IO.File.Exists(patch + file);
        }

        public string Read(string file)
        {
            if(System.IO.File.Exists(patch + file))
                return System.IO.File.ReadAllText(patch + file);
            return null;
        }

        public void Write(string file, string text)
        {
            System.IO.File.WriteAllText(patch + file, text);
        }

        public List<string> Find(string folder)
        {
            if(System.IO.Directory.Exists(patch + folder))
            {
                List<string> files = new List<string>();

                foreach(string file in System.IO.Directory.GetFiles(patch + folder))
                {
                    files.Add(System.IO.Path.GetFileName(file));
                }
                return files;
            }
            return null;
        }

    }
}

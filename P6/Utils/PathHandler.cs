using System;
using System.IO;

namespace Utils
{
    public class PathHandler
    {
        public PathHandler()
        {
            _OS = new OSDetecter();
        }

        private OSDetecter _OS;
        
        public string CodeDir => Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;

        public string SolutionDir => Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

        public string SettingsDir => Path.Combine(SolutionDir, "Settings");
        public string ExerpimentConfigDir => Path.Combine(SettingsDir, "Experiments");

        public string Separator
        {
            get
            {
                if (_OS.IsUnix)
                    return "/";
                else if (_OS.IsWindows)
                    return "\\";
                return "/";
            }
        }

        public string Add(string exising_path, string[] dirs_file)
        {
            string new_path = exising_path;
            for (int i = 0; i < dirs_file.Length; i++)
            {
                new_path += Separator + dirs_file[i];
            }

            return new_path;
        }

        public void CreateDirIfNotExists(string dirPath)
        {
           Directory.CreateDirectory(dirPath);
        }
    }
}
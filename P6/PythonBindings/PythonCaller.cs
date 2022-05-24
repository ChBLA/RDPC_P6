using System;
using System.Collections.Generic;
using System.IO;
using Settings;
using Utils;

namespace PythonBindings
{
    public class PythonCaller
    {
        public PythonCaller()
        {
            _pathHandler = new PathHandler();
            _OS = new OSDetecter();
            AppConfig config = new ConfigManager(Modes.Debug).AppConfig;
            _pyscriptDirPath = _pathHandler.Add(_pathHandler.CodeDir, new string[] {config.PythonScriptsDirName});
        }

        private PathHandler _pathHandler;
        private OSDetecter _OS;
        private string _pyscriptDirPath;

        public string ExecutePythonFile(string fileName, string[] additionalPrependDirs, List<string> arguments = null)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = GetPythonRuntime();
            if (additionalPrependDirs.Length > 0)
            {
                string path = _pathHandler.Add(_pyscriptDirPath, additionalPrependDirs);
                startInfo.ArgumentList.Add(path + _pathHandler.Separator + fileName);
            }
            else
                startInfo.ArgumentList.Add(_pyscriptDirPath + _pathHandler.Separator + fileName);
            foreach (string arg in arguments)
                startInfo.ArgumentList.Add(arg);
            process.StartInfo = startInfo;
            process.Start();

            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            process.WaitForExit();
            return output;

        }

        private string GetPythonRuntime()
        {
            // Naive implementation

            string pyRuntime = "";
            if (_OS.IsWindows)
            {
                pyRuntime = "python";
            }
            else if (_OS.IsUnix)
            {
                pyRuntime = "python3";
            }

            return pyRuntime;
        }
    }
}
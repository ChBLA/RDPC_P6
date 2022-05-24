using System;
using System.IO;
using NLog;

namespace Utils.Logging;

public class Logging
{
    public static void ConfigureNLogger(string level=null)
    {
        string logDir = Path.Combine(new PathHandler().SolutionDir, "Utils", "Logging");
        string logConfigPath = Path.Combine(logDir, "NLog.config");
        Console.WriteLine($"Reading NLog config from: {logConfigPath}");
        var nlogConf = new NLog.Config.XmlLoggingConfiguration(logConfigPath);
        LogManager.Configuration = nlogConf;

        if (level != null)
            OverwriteGlobalLogLevel(level);
    }

    public static void OverwriteGlobalLogLevel(string level)
    {
        LogManager.Configuration.Variables["myLevel"] = level;
        LogManager.ReconfigExistingLoggers(); // Explicit refresh of Layouts and updates active Logger-objects
    }
}
using System;
using System.IO;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using Utils;

namespace Settings.Experiments;

public class ExperimentConfigManager
{
    public OptimizerConfig OptimizerConfig;
    public AppConfig AppConfig;
    private static readonly Logger Logger = LogManager.GetLogger(nameof(ExperimentConfigManager));

    public ExperimentConfigManager(string foldername)
    {
        LoadExperimentConfig(foldername);
    }

    public ExperimentConfigManager()
    {

    }
    
    private void LoadExperimentConfig(string folderName)
    {
        var confPaths= getExperimentFilePath(folderName);
        var appConfFilePath = confPaths.Item1;
        var optConfFilePath = confPaths.Item2;
        Logger.Info($"Loading AppConfig.json from: '{appConfFilePath}'");
        Logger.Info($"Loading OptimizerConfig.json from '{optConfFilePath}'");

        string appConfContent = File.ReadAllText(appConfFilePath);
        string optConfContent = File.ReadAllText(optConfFilePath);
        Logger.Info($"Load AppConfig.json content: '{appConfContent}'");        
        Logger.Info($"Load OptimizerConfig.json content: '{optConfContent}'");
        
        AppConfig = JsonConvert.DeserializeObject<AppConfig>(appConfContent);
        OptimizerConfig = JsonConvert.DeserializeObject<OptimizerConfig>(optConfContent);
    }
    
    private (string , string) getExperimentFilePath(string folderName)
    {
        PathHandler ph = new PathHandler();
        Logger.Info($"Running the experiment specified in '{folderName}'");
        string appConfPath = Path.Combine(ph.ExerpimentConfigDir, folderName, "AppConfig.json");
        string optConfPath = Path.Combine(ph.ExerpimentConfigDir, folderName, "OptimizerConfig.json");
        return (appConfPath, optConfPath);
    }

}
using System.Collections.Concurrent;
using System.Text;
using Experiments;
using Experiments.Interfaces;
using Experiments.Tools;
using GradientDescentAlgorithm.GDOptimisers;
using IdentifiablePoints;
using Newtonsoft.Json;
using NLog;
using PythonBindings;
using Settings;
using Settings.Experiments;
using Utils;
using Utils.Logging;

namespace Main
{
    class Program  
    {
        static AppConfig _appConfig;
        static OptimizerConfig _optimizerConfig;
        static string[] _additionalArgs;
        private static readonly Logger Logger = LogManager.GetLogger(nameof(Main));

        static void Main(string[] args)
        {
            Logging.ConfigureNLogger(); // To disable logging level:"off" 
            
            PrintReceivedArgs(args);
            if (args.Length >= 1)
            {
                 switch (args[0])
                 {
                     case "Experiments":
                        (AppConfig appConfig, OptimizerConfig optimizerConfig) = ExtractConfigurations(args[1..args.Length]);
                        RunExperiment(appConfig, optimizerConfig);
                        break;
                    // other 'projects' can be added here
                    case "Setup":
                        Logger.Info($"Starting processing of ML1M");
                        PythonCaller pyCaller1 = new PythonCaller();
                        var res1 = pyCaller1.ExecutePythonFile("process_ml1m.py", new string[] { "DataProcessing" });
                        Logger.Info($"Starting processing of ML10M");
                        PythonCaller pyCaller2 = new PythonCaller();
                        var res2 = pyCaller2.ExecutePythonFile("process_ml10m.py", new string[] { "DataProcessing" });
                        break;
                    default:
                        Logger.Info("Expecting a project as first argument!");
                        break;
                 }                
            }
            else
            {
                Logger.Info($"No argument was provided. Terminating....");
            }
        }

        private static void RunExperiment(AppConfig appConfig, OptimizerConfig optimizerConfig)
        {
            var results = DetermineAndRunExperiment(appConfig, optimizerConfig);
            return;
        }

        private static IExperimentResult? DetermineAndRunExperiment(AppConfig appConfig, OptimizerConfig optimizerConfig)
        {
            IExperimentResult? result = null;
            PointLoader loader;
            Logger.Info($"ExperimentType: {appConfig.ExperimentType}");
            switch (appConfig.ExperimentType)
            {
                case ExperimentType.TuneDimensions:
                    loader = new PointLoader(appConfig.DataFilePath);
                    var exp = new Tuning(loader, appConfig, optimizerConfig).TuneDimensions();
                    WriteToFile(exp.Item1);
                    exp.Item2();
                    break;
                case ExperimentType.TuneRTDConstant:
                    loader = new PointLoader(appConfig.DataFilePath);
                    var exp_rtdc = new Tuning(loader, appConfig, optimizerConfig).TuneRTDConstant();
                    WriteToFile(exp_rtdc.Item1);
                    exp_rtdc.Item2();
                    break;
                case ExperimentType.TuneRTDPower:
                    loader = new PointLoader(appConfig.DataFilePath);
                    var exp_rtdp = new Tuning(loader, appConfig, optimizerConfig).TuneRTDPower();
                    WriteToFile(exp_rtdp.Item1);
                    exp_rtdp.Item2();
                    break;
                case ExperimentType.TuneOptimisers:
                    loader = new PointLoader(appConfig.DataFilePath);
                    var exp_opt = new Tuning(loader, appConfig, optimizerConfig).TuneOptimisers();
                    foreach (var kvp in exp_opt.Item1)
                        WriteToFile(kvp.Value, appConfig.HistorySaveFilePathNoExt + kvp.Key + appConfig.FileExtensions);
                    exp_opt.Item2();
                    break;
                case ExperimentType.DiffusionBenchmark:
                    loader = new PointLoader(appConfig.DataFilePath);
                    if (_additionalArgs?.Length > 0)
                    {
                        int dg;
                        bool isValidInt = int.TryParse(_additionalArgs[0], out dg);
                        optimizerConfig.DiffusionDegree = isValidInt ? dg : optimizerConfig.DiffusionDegree;
                        Logger.Info($"DiffusionDegree changed to: {optimizerConfig.DiffusionDegree}");
                    }
                    appConfig.SaveFileName += $"{optimizerConfig.DiffusionDegree}_";
                    appConfig.HistoryFileName += $"{optimizerConfig.DiffusionDegree}_";
                    Logger.Info($"SaveFileName changed to: {appConfig.SaveFileName}");
                    Logger.Info($"HistoryFileName changed to: {appConfig.HistoryFileName}\n");
                    var exp_db = new Benchmark(loader, appConfig, optimizerConfig).DiffusionBenchmark();
                    WriteToFile(exp_db.Item1);
                    exp_db.Item2();
                    break;
                case ExperimentType.TrueDiffusion:
                    loader = new PointLoader(appConfig.DataFilePath);
                    if (_additionalArgs?.Length > 0)
                    {
                        int dg;
                        bool isValidInt = int.TryParse(_additionalArgs[0], out dg);
                        optimizerConfig.DiffusionDegree = isValidInt ? dg : optimizerConfig.DiffusionDegree;
                        Logger.Info($"DiffusionDegree changed to: {optimizerConfig.DiffusionDegree}");
                    }
                    appConfig.SaveFileName += $"{optimizerConfig.DiffusionDegree}_";
                    appConfig.HistoryFileName += $"{optimizerConfig.DiffusionDegree}_";
                    Logger.Info($"SaveFileName changed to: {appConfig.SaveFileName}");
                    Logger.Info($"HistoryFileName changed to: {appConfig.HistoryFileName}\n");
                    var exp_td = new Benchmark(loader, appConfig, optimizerConfig).RepeatedTrueDiffusionExperiment(1);
                    WriteToFile(exp_td.Item1);
                    exp_td.Item2();
                    break;
                case ExperimentType.VanillaBenchmark:
                    loader = new PointLoader(appConfig.DataFilePath);
                    var exp_vb = new Benchmark(loader, appConfig, optimizerConfig).VanillaBenchmark(20);
                    WriteToFile(exp_vb.Item1);
                    exp_vb.Item2();
                    break;
                case ExperimentType.TuneLRAndDecay:
                    loader = new PointLoader(appConfig.DataFilePath);
                    if (_additionalArgs?.Length > 0)
                    {
                        int dg;
                        bool isValidInt = int.TryParse(_additionalArgs[0], out dg);
                        optimizerConfig.DiffusionDegree = isValidInt ? dg : optimizerConfig.DiffusionDegree;
                        Logger.Info($"DiffusionDegree changed to: {optimizerConfig.DiffusionDegree}");
                    }
                    appConfig.SaveFileName += $"{optimizerConfig.DiffusionDegree}_";
                    appConfig.HistoryFileName += $"{optimizerConfig.DiffusionDegree}_";
                    Logger.Info($"SaveFileName changed to: {appConfig.SaveFileName}");
                    Logger.Info($"HistoryFileName changed to: {appConfig.HistoryFileName}\n");
                    var exp_lrd = new Tuning(loader, appConfig, optimizerConfig).TuneDiffusionLRAndDecay();
                    WriteToFile(exp_lrd.Item1);
                    exp_lrd.Item2();
                    break;
                case ExperimentType.TuneLRDecayConstant:
                    loader = new PointLoader(appConfig.DataFilePath);
                    if (_additionalArgs?.Length > 0)
                    {
                        int dg;
                        bool isValidInt = int.TryParse(_additionalArgs[0], out dg);
                        optimizerConfig.DiffusionDegree = isValidInt ? dg : optimizerConfig.DiffusionDegree;
                        Logger.Info($"DiffusionDegree changed to: {optimizerConfig.DiffusionDegree}");
                    }
                    appConfig.SaveFileName += $"{optimizerConfig.DiffusionDegree}_";
                    appConfig.HistoryFileName += $"{optimizerConfig.DiffusionDegree}_";
                    Logger.Info($"SaveFileName changed to: {appConfig.SaveFileName}");
                    Logger.Info($"HistoryFileName changed to: {appConfig.HistoryFileName}\n");
                    var exp_dc = new Tuning(loader, appConfig, optimizerConfig).TuneDecayConstant();
                    WriteToFile(exp_dc.Item1);
                    exp_dc.Item2();
                    break;
                case ExperimentType.TuneLRDecayFactor:
                    loader = new PointLoader(appConfig.DataFilePath);
                    if (_additionalArgs?.Length > 0)
                    {
                        int dg;
                        bool isValidInt = int.TryParse(_additionalArgs[0], out dg);
                        optimizerConfig.DiffusionDegree = isValidInt ? dg : optimizerConfig.DiffusionDegree;
                        Logger.Info($"DiffusionDegree changed to: {optimizerConfig.DiffusionDegree}");
                    }
                    appConfig.SaveFileName += $"{optimizerConfig.DiffusionDegree}_";
                    appConfig.HistoryFileName += $"{optimizerConfig.DiffusionDegree}_";
                    Logger.Info($"SaveFileName changed to: {appConfig.SaveFileName}");
                    Logger.Info($"HistoryFileName changed to: {appConfig.HistoryFileName}\n");
                    var exp_df = new Tuning(loader, appConfig, optimizerConfig).TuneDecayFactor();
                    WriteToFile(exp_df.Item1);
                    exp_df.Item2();
                    break;
                case ExperimentType.TuneLRDecayBase:
                    loader = new PointLoader(appConfig.DataFilePath);
                    if (_additionalArgs?.Length > 0)
                    {
                        int dg;
                        bool isValidInt = int.TryParse(_additionalArgs[0], out dg);
                        optimizerConfig.DiffusionDegree = isValidInt ? dg : optimizerConfig.DiffusionDegree;
                        Logger.Info($"DiffusionDegree changed to: {optimizerConfig.DiffusionDegree}");
                    }
                    appConfig.SaveFileName += $"{optimizerConfig.DiffusionDegree}_";
                    appConfig.HistoryFileName += $"{optimizerConfig.DiffusionDegree}_";
                    Logger.Info($"SaveFileName changed to: {appConfig.SaveFileName}");
                    Logger.Info($"HistoryFileName changed to: {appConfig.HistoryFileName}\n");
                    var exp_ldb = new Tuning(loader, appConfig, optimizerConfig).TuneDecayBase();
                    WriteToFile(exp_ldb.Item1);
                    exp_ldb.Item2();
                    break;
                default:
                    throw new Exception($"Unknown experiment type {appConfig.ExperimentType}");
            }

            return result;
        }

        private static (AppConfig, OptimizerConfig) ExtractConfigurations(string[] args)
        {
            ExperimentConfigManager expConf = new ExperimentConfigManager();
            if (args.Length >= 1)
            {
                string experimentFolderName = args[0];
                expConf = new ExperimentConfigManager(experimentFolderName);
            }

            var appConfig = args.Length == 0 ? new ConfigManager(Modes.Debug).AppConfig : expConf.AppConfig;
            var optimizerConfig = args.Length == 0 ? new ConfigManager(Modes.Debug).OptimizerConfig : expConf.OptimizerConfig;

            // Set static fields
            _appConfig = appConfig;
            _optimizerConfig = optimizerConfig;
            _additionalArgs = args[1..args.Length];

            return (appConfig, optimizerConfig);
        } 

        private void tuneExperiments(PointLoader loader)
        {
            var optimisers = new OptimiserType[] { OptimiserType.Adam, OptimiserType.RMSProp, OptimiserType.Nesterov, OptimiserType.Momentum, OptimiserType.Vanilla };
            var lr = new float[] { 2.8f, 1.2f, 0.00052f, 0.00085f, 0.00085f };
            var dn_lr = new float[] { 0.364f, 1.72f, 0.00168f, 0.001f, 0.02494f };
            int optimiser = 0;
            int[] dimensions = new int[] { 7, 30 };
            float[] power = new float[] { 1.5f, 1.9f }, constant = new float[] { 6.4f, 7.2f };
            int dataset = 0;
            string jsonString = "";

            Logger.Info("Starting!");
            for (int i = 0; i < optimisers.Length; i++) //< optimisers.Length
            {
                Logger.Info("Optimiser: " + Enum.GetName(optimisers[i]));
                var results = new PointCloudExperiments(loader).VaryLearningRate((int a) => lr[i], 20, optimisers[i],
                                                                                 dimensions: dimensions[dataset]);
                jsonString = JsonConvert.SerializeObject(results);
                File.WriteAllText(_appConfig.HistorySaveFilePathNoExt + "_vary_lr_rtd_constant_" + Enum.GetName(optimisers[i]) + ".json", jsonString);
                Logger.Info($"Saved!");
            }
        }
        private static void PrintReceivedArgs(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Received args: ");
            for (int i = 0; i < args.Length; i++)
                sb.Append($" arg[{i}]='{args[i]}'");
            Logger.Info(sb.ToString());
        }
        

        private static void WriteToFile(IExperimentResult results)
        {
            string jsonString = results.SerializeResult();
            string savePath = Path.Combine(_appConfig.HistorySaveFilePath);
            new PathHandler().CreateDirIfNotExists(_appConfig.HistoryFolderPath);
            File.WriteAllText(savePath, jsonString);
        }

        private static void WriteToFile(IExperimentResult results, string savePath)
        {
            string jsonString = results.SerializeResult();
            new PathHandler().CreateDirIfNotExists(_appConfig.HistoryFolderPath);
            File.WriteAllText(savePath, jsonString);
        }
    }
}
using IdentifiablePoints;
using Settings;
using Experiments.Interfaces;
using Experiments.Tools;
using GradientDescentAlgorithm.GDOptimisers;
using Newtonsoft.Json;
using PythonBindings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using PointCloudUtil;
using static Experiments.DiffusionExperiment;
using static Experiments.TrueDiffusion;

namespace Experiments
{
    public class Benchmark
    {
        private PointLoader _loader;
        private AppConfig _appConfig;
        private OptimizerConfig _optimizationConfig;

        public Benchmark(PointLoader loader, AppConfig appConfig, OptimizerConfig optimizationConfig)
        {
            _loader = loader;
            _appConfig = appConfig;
            _optimizationConfig = optimizationConfig;
        }

        public (IExperimentResult, Action) VanillaBenchmark(int repitions)
        {
            var results = new List<RunData>();

            Console.WriteLine($"Starting multi-run for {repitions} runs");
            Console.WriteLine($"Saving as: {_appConfig.SaveFilePathNoExt + '#' + _appConfig.FileExtensions}");
            for (int i = 0; i < repitions; i++)
            {
                Console.WriteLine($"Started iteration {i}");
                var setup = new TimedRunner.Setup(_optimizationConfig.VectorDimensions, _appConfig.Iterations, _optimizationConfig.InitialLearningRate,
                    _optimizationConfig.DistanceMethod, _optimizationConfig.GetInverseDistanceMethod());
                var cloud = new PointFactory(_loader).GetPoints(_optimizationConfig.VectorDimensions, 
                    _optimizationConfig.GetDistanceMethod(), _optimizationConfig.GetInverseDistanceMethod(),
                    _optimizationConfig.ValidationSplit, noPrint:true);
                results.Add(TimedRunner.TimedRun(cloud, setup, _optimizationConfig.OptimiserType, _optimizationConfig.UseEarlyStopping));
                PCUtilities.SavePointCloudData(cloud, _appConfig, _optimizationConfig, i);
            }

            var pythonScript = () =>
            {
                var files = new List<string>();
                for (int i = 0; i < repitions; i++)
                    files.Add(_appConfig.SaveFilePathNoExt + i + _appConfig.FileExtensions);
                var arguments = new List<string>() { "-q", "-m", "--rtd", _appConfig.DataType.Equals(DataType.Douban) ? "douban" : "movielens", "--files" };
                arguments.AddRange(files);
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("analyse_point_cloud.py", new string[] { "DataProcessing", "PointCloudAnalysis" }, arguments);
                Console.WriteLine("\nTest RMSE:");
                Console.WriteLine(res);

                var times = new List<string>();
                for (int i = 0; i < repitions; i++)
                    times.Add(results[i].TimeUsed.TotalSeconds.ToString(new System.Globalization.CultureInfo("en-GB")));
                arguments = new List<string>() { "--confidence", "0.95", "--data" };
                arguments.AddRange(times);
                pyCaller = new PythonCaller();
                res = pyCaller.ExecutePythonFile("confidence.py", new string[] { "DataProcessing", "PointCloudAnalysis" }, arguments);
                Console.WriteLine("\nTime Spent (in seconds):");
                Console.WriteLine(res);
            };

            return (new Benchmark3(results), pythonScript);
        }

        public (IExperimentResult, Action) DiffusionBenchmark()
        {
            Func<int, float> lrFunc = (int s) => _optimizationConfig.DecayConstant + 
                _optimizationConfig.DecayFactor * (float)Math.Pow(_optimizationConfig.DecayPowerBase, s);
            int diffusionSteps = _optimizationConfig.UseEarlyStopping ? -1 : _appConfig.Iterations;

            var allRes = new List<Experiment2Rapport>();

            Console.WriteLine($"Starting benchmark for {_appConfig.DataFilePath}");
            var times = new List<TimeSpan>();
            int runCount = 20;
            for (int i = 0; i < runCount; i++)
            {
                var startTime = DateTime.Now;
                var result = new DiffusionExperiment(_loader, _optimizationConfig, _appConfig).SimpleDiffusionRun(
                    (int s) => 50, lrFunc, diffusionDegree: _optimizationConfig.DiffusionDegree, maxSteps: diffusionSteps);
                allRes.Add(result.Item1);
                PCUtilities.SavePointCloudData(result.Item2, _appConfig, _optimizationConfig, i);
                times.Add(DateTime.Now - startTime);
                Console.WriteLine($"Finished optimising {i} after {times[i].TotalSeconds} seconds; " +
                    $"Saved as {_appConfig.SaveFilePathNoExt + i + _appConfig.FileExtensions}");
            }

            Action pythonScript = () =>
            {
                var files = new List<string>();
                for (int i = 0; i < runCount; i++)
                {
                    files.Add(_appConfig.SaveFilePathNoExt + i + _appConfig.FileExtensions);
                }
                string rtd = _optimizationConfig.DistanceMethod.Equals(DistanceMethod.DoubanPower) ? "douban" 
                    : (_optimizationConfig.DistanceMethod.Equals(DistanceMethod.ML1MPower) ? "movielens" : "other");
                var arguments = new List<string>() { "-q", "-m", "--rtd", rtd, "--files" };
                arguments.AddRange(files);
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("analyse_point_cloud.py", new string[] { "DataProcessing", "PointCloudAnalysis" }, arguments);

                var timeArguments = new List<string>() { "--confidence", "0.95", "--data" };
                foreach (var time in times)
                {
                    timeArguments.Add(time.TotalSeconds.ToString(new System.Globalization.CultureInfo("en-GB")));
                }
                pyCaller = new PythonCaller();
                var timeRes = pyCaller.ExecutePythonFile("confidence.py", new string[] { "DataProcessing", "PointCloudAnalysis" }, timeArguments);
                
                Console.WriteLine("Test RMSE:");
                Console.WriteLine(res);
                Console.WriteLine("\nTime spent (in seconds):");
                Console.WriteLine(timeRes);
            };

            Console.WriteLine("\nTimes spent:");
            foreach (var time in times)
                Console.WriteLine(time.TotalSeconds);
            Console.WriteLine("\n");
            return (new Benchmark1(allRes), pythonScript);
        }

        public (IExperimentResult, Action) RepeatedTrueDiffusionExperiment(int repititions)
        {
            var testErrors = new List<float>();
            var times = new List<double>();
            var runs = new List<DiffusionStatistics>();
            var steps = 15;
            var useConcurrency = false;

            for (int i = 0; i < repititions; i++)
            {
                Func<int, float> lrFunc = (int s) => _optimizationConfig.DecayConstant + _optimizationConfig.DecayFactor * (float)Math.Pow(_optimizationConfig.DecayPowerBase, s);
                var run = new TrueDiffusion(_loader, _optimizationConfig).RunTrueDiffusion(_appConfig.DataFilePath, _appConfig.MetaDataFilePath, 
                    (int i) => _appConfig.Iterations, lrFunc, _optimizationConfig.DiffusionDegree, maxSteps:steps, runConcurrent:useConcurrency);
                testErrors.Add(run.DiffusionStepErrors[run.DiffusionStepErrors.Count - 1].Item2);
                times.Add(run.TotalTimeSpent);
                runs.Add(run);
            }

            var pythonScript = () =>
            {
                if (repititions > 1)
                {
                    var errorArguments = new List<string>() { "--confidence", "0.95", "--data" };
                    foreach (var err in testErrors)
                    {
                        errorArguments.Add(err.ToString(new System.Globalization.CultureInfo("en-GB")));
                    }
                    PythonCaller pyCaller = new PythonCaller();
                    var errorRes = pyCaller.ExecutePythonFile("confidence.py", new string[] { "DataProcessing", "PointCloudAnalysis" }, errorArguments);

                    var timeArguments = new List<string>() { "--confidence", "0.95", "--data" };
                    foreach (var time in times)
                    {
                        timeArguments.Add(time.ToString(new System.Globalization.CultureInfo("en-GB")));
                    }
                    pyCaller = new PythonCaller();
                    var timeRes = pyCaller.ExecutePythonFile("confidence.py", new string[] { "DataProcessing", "PointCloudAnalysis" }, timeArguments);

                    Console.WriteLine("Test RMSE:");
                    Console.WriteLine(errorRes);
                    Console.WriteLine("\nTime spent:");
                    Console.WriteLine(timeArguments);
                } else
                {
                    Console.WriteLine("Test RMSE:");
                    Console.WriteLine(testErrors[0]);
                    Console.WriteLine("\nTime spent:");
                    Console.WriteLine(times[0]);
                }
            };
            return (new Benchmark2(runs), pythonScript);
        }

        private class Benchmark1 : IExperimentResult
        {
            private List<Experiment2Rapport> results;

            public Benchmark1(List<Experiment2Rapport> results)
            {
                this.results = results;
            }

            public string SerializeResult() => JsonConvert.SerializeObject(results);
        }

        private class Benchmark2 : IExperimentResult
        {
            private List<DiffusionStatistics> results;

            public Benchmark2(List<DiffusionStatistics> results)
            {
                this.results = results;
            }
            public string SerializeResult() => JsonConvert.SerializeObject(results);
        }
        private class Benchmark3 : IExperimentResult
        {
            private List<RunData> results;

            public Benchmark3(List<RunData> results)
            {
                this.results = results;
            }
            public string SerializeResult() => JsonConvert.SerializeObject(results);
        }

        private class EmptyBenchmark : IExperimentResult
        {
            public string SerializeResult() => "";
        }


    }
}

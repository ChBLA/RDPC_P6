using IdentifiablePoints;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiments.Tools;
using GradientDescentAlgorithm;
using PointCloudUtil.Splitting;
using System.Collections.Concurrent;
using NLog;
using Newtonsoft.Json;
using System.IO;
using PythonBindings;
using Experiments.Interfaces;

namespace Experiments
{
    public class DiffusionExperiment
    {
        private static readonly Logger Logger = LogManager.GetLogger(nameof(DiffusionExperiment));
        private OptimizerConfig _expConfig = new OptimizerConfig()
        {
            DistanceMethod = DistanceMethod.Power,
            ValidationSplit = 0.3f,
            OptimiserType = GradientDescentAlgorithm.GDOptimisers.OptimiserType.Adam,
            VectorDimensions = 7,
            InitialLearningRate = 2.8f,
        };
        private AppConfig _appConfig = new AppConfig()
        {
            Iterations = 1
        };

        private PointLoader _loader;

        public DiffusionExperiment(PointLoader loader)
        {
            _loader = loader;
        }

        public DiffusionExperiment(PointLoader loader, OptimizerConfig expConfig, AppConfig appConfig)
        {
            _expConfig = expConfig;
            _appConfig = appConfig;
            _loader = loader;
        }

        public void BenchmarkDiffusion(AppConfig appConfig, int runCount)
        {
            Console.WriteLine($"Starting benchmark for {appConfig.DataFilePath}");
            var times = new List<TimeSpan>();

            for (int i = 0; i < runCount; i++)
            {
                var startTime = DateTime.Now;
                var result = SimpleDiffusionRun((int s) => 50, (int s) => (0.036f + (0.01f) * (float)Math.Pow(0.85f, s)), diffusionDegree: 4, maxSteps: -1).Item2;
                SavePointCloudData(result, appConfig, i);
                times.Add(DateTime.Now - startTime);
                Console.WriteLine($"Finished optimising {i} after {times[i].TotalSeconds} seconds; Saved as {appConfig.SaveFilePathNoExt + i + appConfig.FileExtensions}");
            }

            var files = new List<string>();
            for (int i = 0; i < runCount; i++)
            {
                Console.WriteLine(appConfig.SaveFilePathNoExt + i + appConfig.FileExtensions);
                files.Add(appConfig.SaveFilePathNoExt + i + appConfig.FileExtensions);
            }
            var arguments = new List<string>() { "-q", "-m", "--files" };
            arguments.AddRange(files);
            PythonCaller pyCaller = new PythonCaller();
            var res = pyCaller.ExecutePythonFile("analyse_point_cloud.py", new string[] { "DataProcessing", "PointCloudAnalysis" }, arguments);
            Console.WriteLine(res);

            Console.WriteLine("\n\nTimes spent:\n");
            foreach (var time in times)
                Console.WriteLine(time.TotalSeconds);
        }

        public ConcurrentDictionary<int, Experiment2Rapport> TestOfVaryingDiffusionDegrees()
        {
            var results = new ConcurrentDictionary<int, Experiment2Rapport>();
            var Providers = Enumerable.Range(1, 4);
            Func<int, float> lrFuncGenerator = (int s) => (0.475f) * (float)Math.Pow(0.4f, s);

            // Optimise concurrently + time measurement
            Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
            {
                var sTIme = DateTime.Now;
                results.AddOrUpdate(currentProvider, SimpleDiffusionRun((int i) => 40, lrFuncGenerator, 0.3f, currentProvider, 7).Item1, (key, oldValue) => oldValue);
                Logger.Info($"Thread ({currentProvider}) stopped after {(DateTime.Now - sTIme).TotalSeconds}");
            });

            return results;
        }

        public Dictionary<int, ConcurrentDictionary<int, Experiment2Rapport>> ContinuousSmallDiffusion(int depth = 5, int exploration = 15)
        {
            var results = new Dictionary<int, ConcurrentDictionary<int, Experiment2Rapport>>();

            Func<int, int, Func<int, float>> lrFuncGenerator = (int i, int ii) =>
                ((int s) => _expConfig.DecayConstant + (i * _expConfig.DecayFactorStep) * (float)Math.Pow(ii * _expConfig.DecayPowerBaseStep, s));

            for (int i = 0; i < exploration; i++)
            {
                var Providers = Enumerable.Range(0, exploration);
                var tempResults = new ConcurrentDictionary<int, Experiment2Rapport>();
                var loopStartTime = DateTime.Now;
                Console.WriteLine($"Started iteration {i}");
                // Optimise concurrently + time measurement
                Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
                {
                    var sTIme = DateTime.Now;
                    tempResults.AddOrUpdate(currentProvider, SimpleDiffusionRun((int i) => 50, 
                        lrFuncGenerator(i, currentProvider), 0.3f, _expConfig.DiffusionDegree, depth).Item1, (key, oldValue) => oldValue);
                    Console.WriteLine($"Thread ({i} {currentProvider}) stopped after {(DateTime.Now - sTIme).TotalSeconds}");
                });
                var loopTime = DateTime.Now - loopStartTime;

                results.Add(i, tempResults);
            }

            return results;
        }

        public Dictionary<int, ConcurrentDictionary<int, Experiment2Rapport>> DualLRDiffusionRun(int lrCount1, int lrCount2)
        {
            var results = new Dictionary<int, ConcurrentDictionary<int, Experiment2Rapport>>();

            Func<int, int, Func<int, float>> lrFuncGenerator = (int i, int ii) =>
                ((int s) => s == 0 ? (i + 1) * 0.02f : (ii + 1) * 0.1f);

            for(int i = 0; i < lrCount1; i++)
            {
                var Providers = Enumerable.Range(0, lrCount2);
                var tempResults = new ConcurrentDictionary<int, Experiment2Rapport>();
                var loopStartTime = DateTime.Now;

                // Optimise concurrently + time measurement
                Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
                {
                    var sTIme = DateTime.Now;
                    tempResults.AddOrUpdate(currentProvider, SimpleDiffusionRun((int i) => 40, lrFuncGenerator(i, currentProvider), 0.3f, 1, 1).Item1, (key, oldValue) => oldValue);
                    Logger.Info($"Thread ({i} {currentProvider}) stopped after {(DateTime.Now - sTIme).TotalSeconds}");
                });
                var loopTime = DateTime.Now - loopStartTime;

                results.Add(i, tempResults);
            }

            return results;
        } 

        public (Experiment2Rapport, PointCloud) SimpleDiffusionRun(Func<int, int> dpIterationsFunc, Func<int, float> dpMergeLRFunc, 
            float mergeLR = -1, int diffusionDegree = 3, int maxSteps = 3)
        {
            // Set up run settings for original and merge runs
            var runSetup = new TimedRunner.Setup(_expConfig.VectorDimensions, _appConfig.Iterations, _expConfig.InitialLearningRate, _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod());
            var mergedRunSetup = new TimedRunner.Setup(_expConfig.VectorDimensions, _appConfig.Iterations, mergeLR == -1 ? _expConfig.InitialLearningRate : mergeLR, _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod());
            
            // Run original cloud for comparison
            var cloud = new PointFactory(_loader).GetPoints(_expConfig.VectorDimensions, _expConfig.GetDistanceMethod(), _expConfig.GetInverseDistanceMethod(), _expConfig.ValidationSplit, true);
            var cloudCopy = cloud.GetCopy();
            var originalRun = TimedRunner.TimedRun(cloud, runSetup);

            // Generate initial subclouds (2^diffusionDegree)
            var cloudCount = (int)Math.Pow(2, diffusionDegree);
            var subclouds = new RandomDisjointSplit(cloudCount, _expConfig.GetInverseDistanceMethod()).Split(cloudCopy);

            // Structure for holding data from diffusion process (key is step)
            var diffusionResults = new Dictionary<int, DiffusionStepData>();

            // Information on dpIterations and dpMergeLR
            var dpIterations = new List<int>();
            var dpMergeLRs = new List<float>();

            // Measures for early stopping
            int noImprovementCounter = 0;
            float minImprovement = _appConfig.DataType.Equals(DataType.Douban) ? -5.0f : -125.0f;
            float currentBestError = float.MaxValue;
            bool useEarlyStopping = false;
            bool earlyStop = false;
            if (maxSteps < 0)
            {
                maxSteps = 100;
                useEarlyStopping = true;
            }
            //Console.WriteLine();
            // Diffusion process
            for (int s = 0; s <= maxSteps && !earlyStop; s++)
            {
                //Console.Write($"\rStarting {s} diffusion step");
                dpIterations.Add(dpIterationsFunc(s));
                dpMergeLRs.Add(dpMergeLRFunc(s));
                // Parallel foreach setup
                var diffusionRunSetup = new TimedRunner.Setup(_expConfig.VectorDimensions, dpIterations[s], 
                    dpMergeLRs[s], _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod()
                    );
                var Providers = Enumerable.Range(0, cloudCount);
                var tempResults = new ConcurrentDictionary<int, RunData>();
                var loopStartTime = DateTime.Now;

                // Optimise concurrently + time measurement
                Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
                {
                    tempResults.AddOrUpdate(currentProvider, TimedRunner.TimedRun(subclouds[currentProvider], diffusionRunSetup), (key, oldValue) => oldValue);
                });
                var loopTime = DateTime.Now - loopStartTime;

                // Only split/merge when not the last step (splitting/merging at last step makes no difference)
                if (s < maxSteps)
                {
                    // Split
                    var splitSubclouds = new List<(PointCloud, PointCloud)>();
                    for (int g = 0; g < cloudCount; g++)
                    {
                        var splitCloud = new RandomDisjointSplit(2, _expConfig.GetInverseDistanceMethod()).Split(subclouds[g]);
                        splitSubclouds.Add((splitCloud[0], splitCloud[1]));
                    }

                    // Merge
                    for (int i = 0; i < cloudCount; i++)
                    {
                        var h = i + (1 - 2 * ((i / (int)(Math.Pow(2, s % diffusionDegree)) % 2))) * (int)Math.Pow(2, s % diffusionDegree);
                        subclouds[i] = splitSubclouds[i].Item1.Combine(splitSubclouds[h].Item2);
                    }
                }

                // Collect total error from all subclouds (merged)
                var mergedTotalError = PointCloud.Collapse(subclouds).GetError().Item1;
                var mergedTotalValError = PointCloud.Collapse(subclouds).GetValError().Item1;
                // Add all results to appropriate structure (tempResults, mergedTotalError, loopTime)
                diffusionResults.Add(s, new DiffusionStepData(tempResults, mergedTotalError, mergedTotalValError, loopTime));

                if (useEarlyStopping)
                {
                    if ((mergedTotalError - currentBestError) >= minImprovement)
                        noImprovementCounter++;
                    else
                    {
                        currentBestError = mergedTotalError;
                        noImprovementCounter = 0;
                    }

                    if (noImprovementCounter > (_appConfig.DataType.Equals(DataType.Douban) ? 4 : 2))
                    {
                        earlyStop = true;
                        Console.WriteLine($"\nEarly stop at {s} iterations with error {mergedTotalError}");
                    }
                }
            }

            // Merged run setup
            var errorSumBeforeMerge = 0.0f;
            var connectionsBeforeMerge = 0;

            foreach (var sc in subclouds)
            {
                var error = sc.GetError();
                errorSumBeforeMerge += error.Item1;
                connectionsBeforeMerge += (int)error.Item2;
            }

            var mergedCloud = PointCloud.Collapse(subclouds);
            (var mergeError, var mergeConnectionCount) = mergedCloud.GetError();
            var resultingCloud = mergedCloud.GetCopy();
            
            // Merge
            var mergeOptTimeStart = DateTime.Now;
            var mergedRun = TimedRunner.TimedRun(mergedCloud, mergedRunSetup);
            var mergeOptTime = DateTime.Now - mergeOptTimeStart;

            // Return all results from process
            return (new Experiment2Rapport(originalRun, mergedRun, diffusionResults, errorSumBeforeMerge, connectionsBeforeMerge,
                mergeError, (int)mergeConnectionCount, mergeOptTime, diffusionDegree, maxSteps, mergeLR, dpIterations, dpMergeLRs),
                resultingCloud);
        }

        private void SavePointCloudData(PointCloud cloud, AppConfig appConfig, int number = -1)
        {
            float[][] positions = new float[cloud.GetPointCount()][];
            string[] ids = new string[cloud.GetPointCount()];

            for (int i = 0; i < cloud.GetPointCount(); i++)
            {
                positions[i] = new float[_expConfig.VectorDimensions];
                var point = cloud.GetValuesAsList()[i];
                for (int j = 0; j < _expConfig.VectorDimensions; j++)
                    positions[i][j] = point.Position[j];
                ids[i] = point.Id;
            }

            var data = new { Positions = positions, Ids = ids };

            string jsonString = JsonConvert.SerializeObject(data);

            File.WriteAllText(number == -1 ? appConfig.SaveFilePath : appConfig.SaveFilePathNoExt + number + appConfig.FileExtensions, jsonString);
        }

        public class Experiment2Rapport : IExperimentResult
        {
            public RunData OriginalRun;
            public RunData MergedRun;
            public Dictionary<int, DiffusionStepData> DiffusionSteps;

            public float ErrorBeforeMerge;
            public int ConnectionsBeforeMerge;
            public float ErrorAfterMerge;
            public int ConnectionsAfterMerge;
            public TimeSpan MergeOptTime;

            public int DiffusionDegree;
            public int MaxSteps;
            public float MergeLR;

            // DP = diffusion process
            public List<int> DPIterations;
            public List<float> DPMergeLR;

            public Experiment2Rapport(RunData originalRun, RunData mergedRun, Dictionary<int, DiffusionStepData> diffusionSteps, 
                float errorBeforeMerge, int connectionsBeforeMerge, float errorAfterMerge, int connectionsAfterMerge, 
                TimeSpan mergeOptTime, int diffusionDegree, int maxSteps, float mergeLR, List<int> dPIterations, 
                List<float> dPMergeLR)
            {
                OriginalRun = originalRun;
                MergedRun = mergedRun;
                DiffusionSteps = diffusionSteps;
                ErrorBeforeMerge = errorBeforeMerge;
                ConnectionsBeforeMerge = connectionsBeforeMerge;
                ErrorAfterMerge = errorAfterMerge;
                ConnectionsAfterMerge = connectionsAfterMerge;
                MergeOptTime = mergeOptTime;
                DiffusionDegree = diffusionDegree;
                MaxSteps = maxSteps;
                MergeLR = mergeLR;
                DPIterations = dPIterations;
                DPMergeLR = dPMergeLR;
            }

            public string SerializeResult()
            {
                throw new NotImplementedException();
            }
        }

        public class DiffusionStepData
        {
            public ConcurrentDictionary<int, RunData> SubcloudRuns;
            public float MergedTotalError;
            public float MergedTotalValError;
            public TimeSpan LoopTime;

            public DiffusionStepData(ConcurrentDictionary<int, RunData> subcloudRuns, float mergedTotalError, 
                float mergedTotalValError, TimeSpan loopTime)
            {
                SubcloudRuns = subcloudRuns;
                MergedTotalError = mergedTotalError;
                MergedTotalValError = mergedTotalValError;
                LoopTime = loopTime;
            }
        }
    }    
}

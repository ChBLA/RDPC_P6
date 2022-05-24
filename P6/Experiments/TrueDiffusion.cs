using Experiments.Tools;
using GradientDescentAlgorithm;
using GradientDescentAlgorithm.GDOptimisers;
using IdentifiablePoints;
using PointCloudUtil.Splitting;
using Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Experiments
{
    public class TrueDiffusion
    {
        private static readonly Logger Logger = LogManager.GetLogger(nameof(TrueDiffusion));
        private PointLoader _loader;
        private OptimizerConfig _expConfig = new OptimizerConfig()
        {
            DistanceMethod = DistanceMethod.Flipped,
            ValidationSplit = 0.3f,
            VectorDimensions = 10
        };

        public TrueDiffusion(PointLoader loader, OptimizerConfig expConfig)
        {
            _loader = loader;
            _expConfig = expConfig;
        }

        public DiffusionStatistics RunTrueDiffusion(string ratingsFile, string metaFile, Func<int, int> dpIterationsFunc,
            Func<int, float> dpMergeLRFunc, int diffusionDegree = 3, int maxSteps = 3, int mu = 2, bool runConcurrent = false)
        {
            var startTime = DateTime.Now;
            var optTime = 0.0;
            var analysisTime = 0.0;
            var errors = new List<(float, float)>();

            // Create initial files to hold data
            int numOfClouds = (int)Math.Pow(mu, diffusionDegree);
            var diffusionIterations = new List<int>[numOfClouds];
            (string subfolder, Dictionary<string, int> pointMap) = PointLoader.DiffusedFileGeneration(ratingsFile, metaFile, numOfClouds, _expConfig.ValidationSplit);
            Func<int, int, int, int> m_mu_k = (int i, int varphi, int rho) => i - 
                ((i / (int)Math.Pow(mu, varphi)) % mu) * (int)Math.Pow(mu, varphi) + rho * (int)Math.Pow(mu, varphi);

            // Create folder for subclouds
            string subcloudsPath = Path.Combine(subfolder, "work_directory");
            if (Directory.Exists(subcloudsPath))
            {
                Logger.Info($"DELETING {subcloudsPath} in 10 seconds");
                Thread.Sleep(10000);
                Directory.Delete(subcloudsPath, true);
            }
            Directory.CreateDirectory(subcloudsPath);

            // Create the halved subclouds
            for (int i = 0; i < numOfClouds; i++)
            {
                if (diffusionIterations[i] is null)
                    diffusionIterations[i] = new List<int>();

                // Load original cloud
                var pointFilter = new List<string>();
                foreach (var (key, val) in pointMap)
                {
                    if (val == i)
                        pointFilter.Add(key);
                }
                var cloud = new PointFactory(new PointLoader(Path.Combine(subfolder, $"rsc_{i}.json")))
                    .GetPoints(_expConfig.VectorDimensions, _expConfig.GetDistanceMethod(), _expConfig.GetInverseDistanceMethod(), 
                        _expConfig.ValidationSplit, filter:pointFilter);
                // Optimise
                var startOptTime = DateTime.Now;
                var optHistory = GradientDescentAlgorithm.GradientDescentAlgorithm.Run(cloud, dpMergeLRFunc(0), dpIterationsFunc(0), 
                    _expConfig.GetInverseDistanceMethod(), new AdamOptimiser(), validationSplit: _expConfig.ValidationSplit, 
                    noPrint: true, enableEarlyStopping:true);
                optTime += (DateTime.Now - startOptTime).TotalSeconds;
                diffusionIterations[i].Add(optHistory.Error.Length);

                // Distribute clouds using m_mu,k
                var subclouds = new RandomDisjointSplit(mu, _expConfig.GetInverseDistanceMethod()).Split(cloud);
                for (int j = 0; j < mu; j++)
                {
                    int digit = (i / (int)Math.Pow(mu, 0)) % mu;
                    string path = Path.Combine(subcloudsPath, $"rsc_{m_mu_k(i, 0, j)}_{digit}_{1}.json");
                    PointLoader.SavePointCloudToFile(path, subclouds[j]);
                }
            }

            var aStartTime = DateTime.Now;
            errors.Add(GetValidationErrorFromMultipleFiles(subcloudsPath, _expConfig.GetInverseDistanceMethod()));
            analysisTime += (DateTime.Now - aStartTime).TotalSeconds;

            // Diffusion steps
            for (int s = 1; s <= maxSteps; s++)
            {
                // Optimise each pair of clouds
                Parallel.For(0, numOfClouds, new ParallelOptions { MaxDegreeOfParallelism = runConcurrent ? numOfClouds : 1 },
                (int i) =>
                {
                    var subclouds = new List<PointCloud>();
                    for (int k = 0; k < mu; k++)
                    {
                        string intermediatePath = Path.Combine(subcloudsPath, $"rsc_{i}_{k}_{s}.json");
                        subclouds.Add(PointLoader.LoadPointCloudFromFile(intermediatePath, _expConfig.GetInverseDistanceMethod()));
                        File.Delete(intermediatePath);
                    }
                    var resCloud = PointCloud.Collapse(subclouds);

                    // Optimise
                    var startOptTime = DateTime.Now;
                    var optHistory = GradientDescentAlgorithm.GradientDescentAlgorithm.Run(resCloud, dpMergeLRFunc(s), dpIterationsFunc(s),
                        _expConfig.GetInverseDistanceMethod(), new AdamOptimiser(), validationSplit: _expConfig.ValidationSplit,
                        noPrint: true, enableEarlyStopping: true);
                    optTime += (DateTime.Now - startOptTime).TotalSeconds;
                    diffusionIterations[i].Add(optHistory.Error.Length);

                    // Save results
                    subclouds = new RandomDisjointSplit(mu, _expConfig.GetInverseDistanceMethod()).Split(resCloud);
                    for (int j = 0; j < mu; j++)
                    {
                        int digit = (i / (int)Math.Pow(mu, s % diffusionDegree)) % mu;
                        string newSubPath = Path.Combine(subcloudsPath, $"rsc_{m_mu_k(i, s % diffusionDegree, j)}_{digit}_{s+1}.json");
                        PointLoader.SavePointCloudToFile(newSubPath, subclouds[j]);
                    }
                });

                // Measure val error 
                aStartTime = DateTime.Now;
                errors.Add(GetValidationErrorFromMultipleFiles(subcloudsPath, _expConfig.GetInverseDistanceMethod()));
                analysisTime += (DateTime.Now - aStartTime).TotalSeconds;
            }

            var totalTime = (DateTime.Now - startTime).TotalSeconds;

            return new DiffusionStatistics(totalTime, optTime, analysisTime, diffusionIterations.ToList(), errors);
        }

        public (float, float) GetValidationErrorFromMultipleFiles(string folderPath, Func<float, float> ratingMethod)
        {
            var initTime = DateTime.Now;
            float error = 0.0f;
            float valError = 0.0f;
            int errorConnections = 0;
            int valErrorConnections = 0;

            var files = Directory.GetFiles(folderPath, "rsc*");

            for (int i = 0; i < files.Length; i++)
            {
                var cloud = PointLoader.LoadPointCloudFromFile(files[i], ratingMethod);

                // Find in-file error
                foreach (var point in cloud.GetValues().Values)
                {
                    foreach (var connection in point.ActiveConnections)
                    {
                        var otherPoint = cloud.FindPointByID(connection.Id);
                        if (otherPoint is not null)
                        {
                            float actualRating = ratingMethod((point.Position - otherPoint.Position).Norm);
                            float desiredRating = ratingMethod(connection.Value);
                            float difference = actualRating - desiredRating;
                            if (connection.IsForValidation && point.Id.CompareTo(connection.Id) > 0)
                            {
                                valError += difference * difference;
                                valErrorConnections++;
                            }
                            else if (!connection.IsForValidation && point.Id.CompareTo(otherPoint.Id) > 0)
                            {
                                error += difference * difference;
                                errorConnections++;
                            }
                        }
                    }
                }

                // Find between-files error
                for (int j = i + 1; j < files.Length; j++)
                {
                    var otherCloud = PointLoader.LoadPointCloudFromFile(files[j], ratingMethod);
                    var points = cloud.GetValues().Values;
                    foreach (var point in points) 
                    {
                        var connections = point.Connections;
                        foreach (var connection in connections)
                        {
                            var otherPoint = otherCloud.FindPointByID(connection.Id);
                            if (otherPoint is not null)
                            {
                                float actualRating = ratingMethod((point.Position - otherPoint.Position).Norm);
                                float desiredRating = ratingMethod(connection.Value);
                                float difference = actualRating - desiredRating;
                                if (connection.IsForValidation)
                                {
                                    valError += difference * difference;
                                    valErrorConnections++;
                                }
                                else
                                {
                                    error += difference * difference;
                                    errorConnections++;
                                }
                            }
                        }
                    }
                }
                Logger.Info($"{i} ended at {(DateTime.Now - initTime).TotalSeconds}");
            }
            Logger.Info($"Time used: {(DateTime.Now - initTime).TotalSeconds}");
            var errorRMSE = (float)Math.Sqrt(error / errorConnections);
            var valRMSE = (float)Math.Sqrt(valError / valErrorConnections);
            return (errorRMSE, valRMSE);
        }

        public class DiffusionStatistics
        {
            public double TotalTimeSpent;
            public double TotalOptimizationTimeSpent;
            public double TotalAnalysisTimeSpent;
            public List<List<int>> DiffusionStepNeededIterations;
            public List<(float, float)> DiffusionStepErrors;

            public DiffusionStatistics(double totalTimeSpent, double totalOptimizationTimeSpent, 
                double totalAnalysisTimeSpent, List<List<int>> diffusionStepNeededIterations, 
                List<(float, float)> diffusionStepErrors)
            {
                TotalTimeSpent = totalTimeSpent;
                TotalOptimizationTimeSpent = totalOptimizationTimeSpent;
                TotalAnalysisTimeSpent = totalAnalysisTimeSpent;
                DiffusionStepNeededIterations = diffusionStepNeededIterations;
                DiffusionStepErrors = diffusionStepErrors;
            }
        }
    }
}

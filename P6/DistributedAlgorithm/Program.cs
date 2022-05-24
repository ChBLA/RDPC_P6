using System;
using System.Collections.Generic;
using GradientDescentAlgorithm;
using GradientDescentAlgorithm.GDOptimisers;
using IdentifiablePoints;
using PointCloudUtil;
using PointCloudUtil.Splitting;
using Settings;

namespace DistributedAlgorithm
{
    class Program
    {
        static AppConfig _appConfig;
        static OptimizerConfig _optConfig;
        static void Main(string[] args)
        {
            _appConfig = new ConfigManager(Modes.Debug).AppConfig;
            _optConfig = new ConfigManager(Modes.Debug).OptimizerConfig;

            var loader = new PointLoader(_appConfig.DataFilePath);
            var points = new PointFactory(loader).GetPoints(_optConfig.VectorDimensions, _optConfig.GetDistanceMethod(), _optConfig.GetInverseDistanceMethod(), _optConfig.ValidationSplit);
            Console.WriteLine("Data loaded and processed\n");

            var strategy = new DisjointSplit(10, _optConfig.GetInverseDistanceMethod());
            var clouds = strategy.Split(points);

            for (int i = 0; i < clouds.Count; i++)
            {
                Console.WriteLine($"\n\nCloud {i}\n-----------------------");
                ConfiguredRun(clouds[i]);
            }

            for (int i = 0; i < clouds.Count; i++) 
            {
                (float error, float connections) = clouds[i].GetError();
                Console.WriteLine($"Cloud {i} error: {error}, connections: {connections} / {clouds[i].GetAllRatingsCount()}, average: {error/connections}");
            }
        }

        static void ConfiguredRun(PointCloud points)
        {
            GradientDescentAlgorithm.GradientDescentAlgorithm.Run(
                points,
                _optConfig.LearningRate / (_optConfig.ProportionalLR ? points.GetPointCount() : 1),
                _appConfig.Iterations,
                _optConfig.GetInverseDistanceMethod(),
                new AdamOptimiser(),
                graphError: true,
                validationSplit: _optConfig.ValidationSplit
            );
        }
    }
}

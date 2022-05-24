using Experiments.Tools;
using IdentifiablePoints;
using Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GradientDescentAlgorithm.GDOptimisers;

namespace Experiments
{
    public class PointCloudExperiments
    {
        private PointLoader _pointLoader;
        private OptimizerConfig _expConfig = new OptimizerConfig()
        {
            DistanceMethod = DistanceMethod.Flipped,
            ValidationSplit = 0.3f
        };

        public PointCloudExperiments(PointLoader pointLoader)
        {
            _pointLoader = pointLoader;
        }

        public PointCloudExperiments(PointLoader pointLoader, OptimizerConfig expConfig) : this(pointLoader)
        {
            _expConfig = expConfig;
        }



        // dimInterval is the smallest and largest number of dimensions, respectively, that is [start, end]
        public ConcurrentDictionary<int, RunData> VaryDimensions((int, int) dimInterval)
        {
            
            var results = new ConcurrentDictionary<int, RunData>();
            var Providers = Enumerable.Range(dimInterval.Item1, dimInterval.Item2 - dimInterval.Item1 + 1);

            Parallel.ForEach(Providers, currentProvider =>
            {
                var runSettings = new TimedRunner.Setup(currentProvider, 100, 3.0f, _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod());
                var cloud = new PointFactory(_pointLoader).GetPoints(currentProvider, _expConfig.GetDistanceMethod(), _expConfig.GetInverseDistanceMethod(), _expConfig.ValidationSplit, true);
                results.AddOrUpdate(currentProvider, TimedRunner.TimedRun(cloud, runSettings), (key, oldValue) => oldValue);
            });

            return results;
        }

        public RunData SingleRun()
        {
            float LR = 1e-4f;
            int dimensions = 10;
            var runSettings = new TimedRunner.Setup(dimensions, 100, LR, _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod());
            var cloud = new PointFactory(_pointLoader).GetPoints(dimensions, _expConfig.GetDistanceMethod(),
                                                           _expConfig.GetInverseDistanceMethod(), _expConfig.ValidationSplit, true);
            return TimedRunner.TimedRun(cloud, runSettings, optimiser: OptimiserType.Adam);
        }

        public ConcurrentDictionary<float, RunData> VaryLearningRate(Func<int, float> GetLRStep, int explorationSteps, 
            OptimiserType optimiser, int dimensions = 5, int iterations = 100, float fraction = 1.0f)
        {
            var results = new ConcurrentDictionary<float, RunData>();
            var Providers = Enumerable.Range(0, explorationSteps);

            Parallel.ForEach(Providers, currentProvider =>
            {
                float LR = GetLRStep(currentProvider);
                var runSettings = new TimedRunner.Setup(dimensions, iterations, LR, _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod());
                var cloud = new PointFactory(_pointLoader).GetPoints(dimensions, _expConfig.GetDistanceMethod(), _expConfig.GetInverseDistanceMethod(), _expConfig.ValidationSplit, true).GetSubCloud(fraction);
                results.AddOrUpdate(currentProvider, TimedRunner.TimedRun(cloud, runSettings, optimiser: optimiser), (key, oldValue) => oldValue);
                Console.WriteLine(currentProvider);
            });

            return results;
        }

        public Dictionary<int, ConcurrentDictionary<float, RunData>> VaryRTDConstant(Func<int, float> getLR, 
            int lrExSteps, float power, Func<int, float> getRTDConstant, int rtdExSteps,
            OptimiserType optimiser, int dimensions, int iterations = 100)
        {
            var all_results = new Dictionary<int, ConcurrentDictionary<float, RunData>>();

            for (int i = 0; i < lrExSteps; i++)
            {
                Console.WriteLine($"Starting: {i}");
                var results = new ConcurrentDictionary<float, RunData>();
                var Providers = Enumerable.Range(0, rtdExSteps);

                Parallel.ForEach(Providers, currentProvider =>
                {
                    float constant = getRTDConstant(currentProvider);
                    float rtd(float a) => (float)Math.Pow(constant - a, power);
                    float inv_rtd(float a) => (float)(constant - Math.Pow(a, 1f / power));
                    var runSettings = new TimedRunner.Setup(dimensions, iterations, getLR(i), DistanceMethod.Power, inv_rtd);
                    var cloud = new PointFactory(_pointLoader).GetPoints(dimensions, rtd, inv_rtd, _expConfig.ValidationSplit, true);
                    results.AddOrUpdate(currentProvider, TimedRunner.TimedRun(cloud, runSettings, optimiser: optimiser), (key, oldValue) => oldValue);
                });

                all_results.Add(i, results);
            }
            

            return all_results;
        }

        public Dictionary<int, ConcurrentDictionary<float, RunData>> VaryRTDPower(Func<int, float> getLR,
            int lrExSteps, float constant, Func<int, float> getRTDPower, int rtdExSteps,
            OptimiserType optimiser, int dimensions, int iterations = 100)
        {
            var all_results = new Dictionary<int, ConcurrentDictionary<float, RunData>>();

            for (int i = 0; i < lrExSteps; i++)
            {
                Console.WriteLine($"Starting: {i}");
                var results = new ConcurrentDictionary<float, RunData>();
                var Providers = Enumerable.Range(0, rtdExSteps);

                Parallel.ForEach(Providers, currentProvider =>
                {
                    float power = getRTDPower(currentProvider);
                    float rtd(float a) => (float)Math.Pow(constant - a, power);
                    float inv_rtd(float a) => (float)(constant - Math.Pow(a, 1f / power));
                    var runSettings = new TimedRunner.Setup(dimensions, iterations, getLR(i), DistanceMethod.Power, inv_rtd);
                    var cloud = new PointFactory(_pointLoader).GetPoints(dimensions, rtd, inv_rtd, _expConfig.ValidationSplit, true);
                    results.AddOrUpdate(currentProvider, TimedRunner.TimedRun(cloud, runSettings, optimiser: optimiser), (key, oldValue) => oldValue);
                });

                all_results.Add(i, results);
            }

            return all_results;
        }

        public Dictionary<int, ConcurrentDictionary<float, RunData>> VaryDimensionAndLearningRate((int, int) dimInteval, 
            Func<int, float> GetLRStep, int explorationSteps, OptimiserType optimiser, int iterations = 100)
        {
            var results = new Dictionary<int, ConcurrentDictionary<float, RunData>>();            
         
            for (int i = dimInteval.Item1; i <= dimInteval.Item2; i++) 
            {
                Console.WriteLine($"Starting: {i}");
                results.Add(i, VaryLearningRate(GetLRStep, explorationSteps, optimiser, dimensions: i, iterations: iterations));
            }

            return results;
        }

        public Dictionary<float, ConcurrentDictionary<float, RunData>> VaryPointCloudSizeAndLearningRate(
            Func<int, float> GetFractionFromStep, Func<int, float> GetLRStep, (int, int) exploration)
        {
            var results = new Dictionary<float, ConcurrentDictionary<float, RunData>>();

            for (int i = 0; i < exploration.Item1; i++)
            {
                float fraction = GetFractionFromStep(i);
                results.Add(fraction, VaryLearningRate(GetLRStep, exploration.Item2, OptimiserType.Adam, fraction: fraction));
            }

            return results;
        }
    }
}

using GradientDescentAlgorithm;
using GradientDescentAlgorithm.GDOptimisers;
using IdentifiablePoints;
using PointCloudUtil.Interfaces;
using PointCloudUtil.Splitting;
using Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiments.Tools;


namespace Experiments
{
    public class SplittingExperimentOne
    {
        /* Split one cloud into n. 
         * Optimise all n sub clouds. 
         * Merge all n sub clouds at once. 
         * Optimise again. 
         * Measure at all times
         */

        private OptimizerConfig _expConfig = new OptimizerConfig() 
        { 
            DistanceMethod = DistanceMethod.Flipped, ValidationSplit = 0.3f 
        };
        private int _dimensions = 10;
        private float _lr = 3.0f;
        private int _iterations = 1;
        private PointLoader _loader;

        public SplittingExperimentOne(PointLoader loader)
        {
            _loader = loader;
        }

        public Experiment1Rapport SimpleNWaySplitting(int ways = 2, float mergeLR = -1)
        {
            var runSetup = new TimedRunner.Setup(_dimensions, _iterations, _lr, _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod());
            var mergedRunSetup = new TimedRunner.Setup(_dimensions, _iterations, mergeLR == -1 ? _lr : mergeLR, _expConfig.DistanceMethod, _expConfig.GetInverseDistanceMethod());
            var cloud = new PointFactory(_loader).GetPoints(_dimensions, _expConfig.GetDistanceMethod(), _expConfig.GetInverseDistanceMethod(), _expConfig.ValidationSplit);
            var cloudCopy = cloud.GetCopy();
            var subClouds = new RandomDisjointSplit(ways, _expConfig.GetInverseDistanceMethod()).Split(cloudCopy);

            var originalRun = TimedRunner.TimedRun(cloud, runSetup);
            var subCloudRuns = new ConcurrentBag<RunData>();

            var Providers = Enumerable.Range(0, ways);
            var loopStartTime = DateTime.Now;
            Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
            {
                subCloudRuns.Add(TimedRunner.TimedRun(subClouds[currentProvider], runSetup));
            });
            var loopTime = DateTime.Now - loopStartTime;

            var errorSumBeforeMerge = 0.0f;
            var connectionsBeforeMerge = 0;

            foreach (var sc in subClouds)
            {
                var error = sc.GetError();
                errorSumBeforeMerge += error.Item1;
                connectionsBeforeMerge += (int)error.Item2;
            }

            // Merge
            var mergeTimeStart = DateTime.Now;
            var mergedCloud = PointCloud.Collapse(subClouds);
            var mergeTime = DateTime.Now - mergeTimeStart;

            (var mergeError, var mergeConnectionCount) = mergedCloud.GetError();
            var mergedRun = TimedRunner.TimedRun(mergedCloud, mergedRunSetup);

            return new Experiment1Rapport(
                originalRun, mergedRun, subCloudRuns.ToList(), loopTime, errorSumBeforeMerge, connectionsBeforeMerge,
                mergeError, (int)mergeConnectionCount, mergeTime
                );
        }

        public ConcurrentDictionary<float, Experiment1Rapport> FindMergeLRConcurrently(int ways = 2)
        {
            Func<int, float> getMergeLR = (int i) => 3.0f / (float)Math.Pow(2, i / 2.0f);

            var Providers = Enumerable.Range(0, 9);
            var results = new ConcurrentDictionary<float, Experiment1Rapport>();
            Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
            {
                var mergeLR = (float)Math.Round(getMergeLR(currentProvider), 4);
                results.AddOrUpdate(mergeLR, SimpleNWaySplitting(ways, mergeLR), (key, oldValue) => oldValue);
            });

            return results;
        }

        public ConcurrentDictionary<int, Experiment1Rapport> VaryingWaySplitMerge()
        {
            var Providers = Enumerable.Range(2, 9);
            var results = new ConcurrentDictionary<int, Experiment1Rapport>();
            Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
            {
                results.AddOrUpdate(currentProvider, SimpleNWaySplitting(currentProvider, 0.3f), (key, oldValue) => oldValue);
            });

            return results;
        }

        public List<ConcurrentDictionary<float, Experiment1Rapport>> VaryWaysAndMergeLR()
        {
            var results = new List<ConcurrentDictionary<float, Experiment1Rapport>>();
            var ways = Enumerable.Range(2, 10);

            foreach (var way in ways)
                results.Add(FindMergeLRConcurrently(way));

            return results;
        }

        public class Experiment1Rapport
        {
            public RunData OriginalRun;
            public RunData MergedRun;
            public List<RunData> SubcloudRuns;
            public TimeSpan SubcloudActualTime;
            public float ErrorBeforeMerge;
            public int ConnectionsBeforeMerge;
            public float ErrorAfterMerge;
            public int ConnectionsAfterMerge;
            public TimeSpan MergeTime;

            public Experiment1Rapport(RunData originalRun, RunData mergedRun, List<RunData> subcloudRuns, 
                TimeSpan subcloudActualTime, float errorBeforeMerge, int connectionsBeforeMerge, 
                float errorAfterMerge, int connectionsAfterMerge, TimeSpan mergeTime)
            {
                OriginalRun = originalRun;
                MergedRun = mergedRun;
                SubcloudRuns = subcloudRuns;
                SubcloudActualTime = subcloudActualTime;
                ErrorBeforeMerge = errorBeforeMerge;
                ConnectionsBeforeMerge = connectionsBeforeMerge;
                ErrorAfterMerge = errorAfterMerge;
                ConnectionsAfterMerge = connectionsAfterMerge;
                MergeTime = mergeTime;
            }
        }
    }
}

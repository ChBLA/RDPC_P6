using GradientDescentAlgorithm;
using GradientDescentAlgorithm.GDOptimisers;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiments.Tools
{
    public class TimedRunner
    {

        public static RunData TimedRun(PointCloud cloud, Setup setup, OptimiserType optimiser = OptimiserType.Adam, bool earlyStop = false)
        {
            var data = new RunData(cloud.GetPointCount(), setup.Dimensions, setup.DistanceMethod,
                cloud.ValidationSplit, optimiser, setup.Iterations);

            var startTime = DateTime.Now;
            data.RunHistory = GradientDescentAlgorithm.GradientDescentAlgorithm.Run(
                        cloud,
                        setup.LearningRate,
                        setup.Iterations,
                        setup.InverseDistanceMethod,
                        optimiser.GetOptimiser(),
                        graphError: false,
                        validationSplit: cloud.ValidationSplit,
                        noPrint: true,
                        enableEarlyStopping: earlyStop
                    );
            data.TimeUsed = DateTime.Now - startTime;

            return data;
        }

        public class Setup
        {
            public int Dimensions;
            public int Iterations;
            public float LearningRate;
            public DistanceMethod DistanceMethod;
            public Func<float, float> InverseDistanceMethod;

            public Setup(int dimensions, int iterations, float learningRate, DistanceMethod distanceMethod, 
                Func<float, float> inverseDistanceMethod)
            {
                Dimensions = dimensions;
                Iterations = iterations;
                LearningRate = learningRate;
                DistanceMethod = distanceMethod;
                InverseDistanceMethod = inverseDistanceMethod;
            }
        }
    }
}

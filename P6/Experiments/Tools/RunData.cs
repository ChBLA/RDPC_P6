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
    public class RunData
    {
        public TimeSpan TimeUsed;
        public int PointCount;
        public History RunHistory;
        public int Dimensions;
        public DistanceMethod DistanceMethod;
        public float ValidationSplit;
        public OptimiserType OptimiserType;
        public int Iterations;

        public RunData(int pointCount, int dimensions, DistanceMethod distanceMethod, float validationSplit, OptimiserType optimiserType, int iterations)
        {
            PointCount = pointCount;
            Dimensions = dimensions;
            DistanceMethod = distanceMethod;
            ValidationSplit = validationSplit;
            OptimiserType = optimiserType;
            Iterations = iterations;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}

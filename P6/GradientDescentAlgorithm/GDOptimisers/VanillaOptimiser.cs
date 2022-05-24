using GradientDescentAlgorithm.Interfaces;
using LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentAlgorithm.GDOptimisers
{
    public class VanillaOptimiser : IGradientDescentStrategy
    {

        public (float, float) UpdatePoint(PointCloud cloud, DataPoint point, float lr, Func<float, float> getRatingFromDistance)
        {
            (Vector gradient, float error, float valError) = point.GetGradientAndErrors(point.Position, cloud, getRatingFromDistance);

            point.Position -= lr * gradient;

            return (error, valError);
        }

        public new OptimiserType GetType()
        {
            return OptimiserType.Vanilla;
        }
    }
}

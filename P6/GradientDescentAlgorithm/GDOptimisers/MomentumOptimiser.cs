using GradientDescentAlgorithm.Interfaces;
using LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentAlgorithm.GDOptimisers
{
    public class MomentumOptimiser : IGradientDescentStrategy
    {
        private float _beta;

        public MomentumOptimiser(float beta = 0.9f)
        {
            _beta = beta;
        }

        public (float, float) UpdatePoint(PointCloud cloud, DataPoint point, float lr, Func<float, float> getRatingFromDistance)
        {
            (Vector gradient, float error, float valError) = point.GetGradientAndErrors(point.Position, cloud, getRatingFromDistance);

            point.Momentum = point.Momentum * _beta + gradient;
            point.Position -= lr * point.Momentum;

            return (error, valError);
        }

        public new OptimiserType GetType()
        {
            return OptimiserType.Momentum;
        }
    }
}

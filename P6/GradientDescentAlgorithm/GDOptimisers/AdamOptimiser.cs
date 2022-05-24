using GradientDescentAlgorithm.Interfaces;
using LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentAlgorithm.GDOptimisers
{
    public class AdamOptimiser : IGradientDescentStrategy
    {
        private float _beta1;
        private float _beta2;
        private float _epsilon;

        public AdamOptimiser(float beta1 = 0.9f, float beta2 = 0.999f, float epsilon = 1e-8f)
        {
            _beta1 = beta1;
            _beta2 = beta2;
            _epsilon = epsilon;
        }

        public (float, float) UpdatePoint(PointCloud cloud, DataPoint point, float lr, Func<float, float> getRatingFromDistance)
        {
            (Vector gradient, float error, float valError) = point.GetGradientAndErrors(point.Position, cloud, getRatingFromDistance);

            point.Momentum = _beta1 * point.Momentum + (1 - _beta1) * gradient;
            point.Velocity = _beta2 * point.Velocity + (1 - _beta2) * gradient.SquareElements();
            point.Position -= Vector.Adam(point.Momentum, point.Velocity, lr, _beta1, _beta2, _epsilon);

            return (error, valError);
        }

        public new OptimiserType GetType()
        {
            return OptimiserType.Adam;
        }
    }
}

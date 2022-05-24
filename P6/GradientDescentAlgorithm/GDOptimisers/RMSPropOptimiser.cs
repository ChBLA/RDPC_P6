using GradientDescentAlgorithm.Interfaces;
using LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentAlgorithm.GDOptimisers
{
    public class RMSPropOptimiser : IGradientDescentStrategy
    {
        private float _rho;
        private float _epsilon;

        public RMSPropOptimiser(float rho = 0.99f, float epsilon = 1e-8f)
        {
            _rho = rho;
            _epsilon = epsilon;
        }

        public (float, float) UpdatePoint(PointCloud cloud, DataPoint point, float lr, Func<float, float> getRatingFromDistance)
        {
            (Vector gradient, float error, float valError) = point.GetGradientAndErrors(point.Position, cloud, getRatingFromDistance);

            point.Velocity = _rho * point.Velocity + (1 - _rho) * gradient.SquareElements();
            var change = Vector.RMSProp(point.Velocity, lr, _epsilon, gradient);
            point.Position -= change;

            return (error, valError);
        }

        public new OptimiserType GetType()
        {
            return OptimiserType.RMSProp;
        }
    }
}

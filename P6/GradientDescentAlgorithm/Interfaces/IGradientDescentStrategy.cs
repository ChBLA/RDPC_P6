using GradientDescentAlgorithm.GDOptimisers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentAlgorithm.Interfaces
{
    public interface IGradientDescentStrategy
    {
        (float, float) UpdatePoint(PointCloud cloud, DataPoint point, float lr, Func<float, float> getRatingFromDistance);
        OptimiserType GetType();
    }
}

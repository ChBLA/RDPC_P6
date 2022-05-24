using System.Collections.Generic;
using GradientDescentAlgorithm;

namespace PointCloudUtil.Interfaces
{
    public interface ISplitStrategy
    {
        public List<PointCloud> Split(PointCloud cloud);
    }
}

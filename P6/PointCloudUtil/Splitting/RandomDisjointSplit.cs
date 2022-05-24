using GradientDescentAlgorithm;
using PointCloudUtil.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudUtil.Splitting
{
    public class RandomDisjointSplit : ISplitStrategy
    {
        private int _ways;
        private readonly Func<float, float> _getRatingFromDistance;

        public RandomDisjointSplit(int ways, Func<float, float> getRatingFromDistance)
        {
            _ways = ways;
            _getRatingFromDistance = getRatingFromDistance;
        }

        public List<PointCloud> Split(PointCloud cloud)
        {
            PointCloud workCloud = cloud.GetCopy();
            List<PointCloud> res = new List<PointCloud>();

            List<Dictionary<string, DataPoint>> tempClouds = new List<Dictionary<string, DataPoint>>();
            Initialize(tempClouds);
            int counter = 0;

            while (!workCloud.IsEmpty)
            {
                List<DataPoint> points = workCloud.GetValuesAsList();
                Random rand = new Random();
                DataPoint selectedPoint = points[rand.Next(points.Count)];

                tempClouds[counter % _ways].Add(selectedPoint.Id, selectedPoint);
                workCloud.Remove(selectedPoint);

                counter++;
            }

            foreach (var subCloud in tempClouds)
                res.Add(new PointCloud(subCloud, _getRatingFromDistance));

            return res;
        }

        private void Initialize(List<Dictionary<string, DataPoint>> tempClouds)
        {
            for (int i = 1; i <= _ways; i++)
                tempClouds.Add(new Dictionary<string, DataPoint>());
        }
    }
}

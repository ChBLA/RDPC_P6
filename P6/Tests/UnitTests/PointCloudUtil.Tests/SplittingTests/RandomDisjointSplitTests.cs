using System.Collections.Generic;
using GradientDescentAlgorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PointCloudUtil.Splitting;
using Settings;

namespace Tests.UnitTests.PointCloudUtil.Tests.SplittingTests
{
    [TestClass]
    public class RandomDisjointSplitTests
    {
        [TestMethod]
        public void RandomSplit__TwoWaySplit__expectPointCountUnchanged()
        {
            OptimizerConfig optimizerConfig = new OptimizerConfig();
            
            // format: user_id, movie_id, rating
            List<(string, string, float)> rawPoints = new List<(string, string, float)>()
            {
                ("u1", "m1", 5),
                ("u2", "m1", 4),
                ("u3", "m2", 2),
                ("u4", "m2", 2)
            };
            DataPoint u1 = new DataPoint(new List<float>() { 1, 1, 1 }, rawPoints[0].Item1);
            DataPoint u2 = new DataPoint(new List<float>() { 0, 0, 1 }, rawPoints[1].Item1);
            DataPoint u3 = new DataPoint(new List<float>() { 2, 2, 2 }, rawPoints[2].Item1);
            DataPoint u4 = new DataPoint(new List<float>() { 0, 0, 2 }, rawPoints[3].Item1);
            
            DataPoint m1 = new DataPoint(new List<float>() { 1, 1, 1 }, rawPoints[0].Item2);
            DataPoint m2 = new DataPoint(new List<float>() { 1, 1, 1 }, rawPoints[2].Item2);

            List<DataPoint> users = new List<DataPoint>() { u1, u2, u3, u4 };
            List<DataPoint> movies = new List<DataPoint>() { m1, m2 };

            Dictionary<string, DataPoint> points = Utilities.GetPointMap(users, movies);

            Utilities.Connect(u1, m1, rawPoints[0]);
            Utilities.Connect(u2, m1, rawPoints[1]);
            Utilities.Connect(u3, m2, rawPoints[2]);
            Utilities.Connect(u4, m2, rawPoints[3]);

            PointCloud pointCloud = new PointCloud(points, optimizerConfig.GetInverseDistanceMethod());
            RandomDisjointSplit randomDisjointSplit = new RandomDisjointSplit(2, optimizerConfig.GetInverseDistanceMethod());
            int expectedPointCount = pointCloud.GetPointCount();

            List<PointCloud> actualClouds = randomDisjointSplit.Split(pointCloud);
            int actualPointCount = 0;
            actualClouds.ForEach(c => actualPointCount += c.GetPointCount());

            Assert.AreEqual(actualPointCount, expectedPointCount);
        }
    }
}
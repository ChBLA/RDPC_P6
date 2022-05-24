using System;
using System.Collections.Generic;
using GradientDescentAlgorithm;

namespace Tests.UnitTests.PointCloudUtil.Tests.SplittingTests
{
    public static class Utilities
    {
        public static float GetSquaredDist(float f)
        {
            return (float)Math.Pow((6.0f - f), 2.0);
        }

        public static Dictionary<string, DataPoint> GetPointMap(List<DataPoint> users, List<DataPoint> movies)
        {
            Dictionary<string, DataPoint> pointMap = new Dictionary<string, DataPoint>();

            foreach (DataPoint user in users)
                pointMap.Add(user.Id, user);

            foreach (DataPoint movie in movies)
                pointMap.Add(movie.Id, movie);

            return pointMap;
        }

        public static Dictionary<string, DataPoint> CreatePointMap(List<DataPoint> dataPoints)
        {
            Dictionary<string, DataPoint> map = new Dictionary<string, DataPoint>();
            foreach (DataPoint dataPoint in dataPoints)
                map.Add(dataPoint.Id, dataPoint);
            return map;
        }

        public static void Connect(DataPoint user, DataPoint movie, (string, string, float) rawPoint)
        {
            if (rawPoint.Item1 != user.Id && rawPoint.Item2 != movie.Id)
                throw new ArgumentException($"Please ensure the that user and movie are in the raw datapoint. " +
                                            $"Got user user.id: {user.Id}, movie.id: {movie.Id}, rawPoint: {rawPoint}");
            
            float distanceBetween = GetSquaredDist(rawPoint.Item3);
            Connection movieCon = new Connection(movie.Id, distanceBetween);
            Connection userCon = new Connection(user.Id, distanceBetween);
            
            user.Connections.Add(movieCon);
            movie.Connections.Add(userCon);
        }

    }
}
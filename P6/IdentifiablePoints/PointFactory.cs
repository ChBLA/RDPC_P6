using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinearAlgebra;
using GradientDescentAlgorithm;
using NLog;

namespace IdentifiablePoints
{
    public class PointFactory
    {
        private PointLoader _map;
        private static readonly Logger Logger = LogManager.GetLogger(nameof(PointFactory));

        public PointFactory(PointLoader map)
        {
            _map = map;
        }

        public PointCloud GetPoints(int dimensions, Func<float, float> calcDesiredDistance, 
                                      Func<float, float> getRatingFromDistance, float validationSplit = 0.0f, bool noPrint = false,
                                      List<string> filter = null)
        {
            Dictionary<string, DataPoint> pointMap = new Dictionary<string, DataPoint>();

            int markedForValidation = 0, itemCount = 0, userCount = 0, connections = 0, activeConnections = 0;

            foreach ((string, string, float, int) connection in _map.Connections)
            {
                DataPoint p1, p2;
                if (!pointMap.ContainsKey(connection.Item1) && (filter == null || filter.Contains(connection.Item1)))
                {
                    p1 = new DataPoint(Vector.GetUniformVector(dimensions, 20), connection.Item1);
                    pointMap.Add(connection.Item1, p1);
                    userCount++;
                } 
                else
                    p1 = pointMap.GetValueOrDefault(connection.Item1, null);

                if (!pointMap.ContainsKey(connection.Item2) && (filter == null || filter.Contains(connection.Item2)))
                {
                    p2 = new DataPoint(Vector.GetUniformVector(dimensions, 20), connection.Item2);
                    pointMap.Add(connection.Item2, p2);
                    itemCount++;
                }
                else
                    p2 = pointMap.GetValueOrDefault(connection.Item2, null);

                bool isForValidation = connection.Item4 == 0 ? false : (connection.Item4 == 1 ? true : markedForValidation / (connections + 0.00001f) < validationSplit);
                float distanceBetweenP1P2 = calcDesiredDistance(connection.Item3);
                if (p1 is not null)
                {
                    Connection c1 = new Connection(connection.Item2, distanceBetweenP1P2, isForValidation);
                    p1.Connections.Add(c1);
                    connections++;
                    if (isForValidation) markedForValidation++;
                }

                if (p2 is not null)
                {
                    Connection c2 = new Connection(connection.Item1, distanceBetweenP1P2, isForValidation);
                    p2.Connections.Add(c2);
                    connections++;
                    if (isForValidation) markedForValidation++;
                }

                if (p1 is not null && p2 is not null)
                    activeConnections++;
            }
            

            // Percentaged of number of connections out of the maximum number of connections
            if (!noPrint)
            {
                Logger.Info($"Data: {itemCount} items and {userCount} users, with {activeConnections} connections. " +
                                  $"Connectedness: {100f * activeConnections / userCount / itemCount}%");
                Logger.Info($"Actual validation split: {1.0f * markedForValidation / connections}");
            }

            return new PointCloud(pointMap, getRatingFromDistance);
        }



    }
}

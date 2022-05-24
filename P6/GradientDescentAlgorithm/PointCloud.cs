using LinearAlgebra;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GradientDescentAlgorithm
{
    public class PointCloud : IEnumerable
    {
        private Dictionary<string, DataPoint> _values;
        public readonly Func<float, float> GetRatingFromDistance;
        private int _connectionCount;
        public readonly int Dimensions;
        public readonly float ValidationSplit;
        public PointCloud(Dictionary<string, DataPoint> values, Func<float, float> getRatingFromDistance)
        {
            _values = values;
            GetRatingFromDistance = getRatingFromDistance;
            Dimensions = values.ElementAt(0).Value.Position.Length;
            _connectionCount = 0;
            var valConnectionCount = 0;
            foreach (var kvp in _values)
            {
                List<Connection> activeConnections = new List<Connection>();
                foreach (var connection in kvp.Value.Connections)
                {
                    if (_values.ContainsKey(connection.Id))
                    {
                        activeConnections.Add(connection);
                        if (connection.IsForValidation)
                            valConnectionCount++;
                    }
                }
                kvp.Value.ActiveConnections = activeConnections;
                _connectionCount += activeConnections.Count;
            }
            ValidationSplit = valConnectionCount / (float)_connectionCount;
        }

        [JsonIgnore]
        public bool IsEmpty
        {
            get => _values.Count == 0; 
        }

        public int GetConnectionCount()
        {
            return _connectionCount;
        }

        public IEnumerator GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        public DataPoint this[string id]
        {
            get
            {
                return _values[id];
            }
        }

        public int GetPointCount()
        {
            return _values.Count;
        }

        public List<DataPoint> GetValuesAsList()
        {
            return _values.Values.ToList();
        }

        public int GetInactiveRatingsCount()
        {
            int num = 0;
            foreach (DataPoint point in _values.Values)
            {
                num += point.Connections.Count - point.ActiveConnections.Count;
            }
            return num / 2;
        }

        public int GetActiveRatingsCount()
        {
            int num = 0;
            foreach (DataPoint point in _values.Values)
            {
                num += point.ActiveConnections.Count;
            }
            return num / 2;
        }

        // Both active and inactive connections
        public int GetAllRatingsCount()
        {
            int num = 0;
            foreach (DataPoint point in _values.Values)
            {
                num += point.Connections.Count;
            }
            return num / 2;
        }

        public (float, float) GetError()
        {
            float error = 0;
            int numOfConnections = 0;

            foreach (DataPoint point in _values.Values)
            {
                foreach (Connection connection in point.ActiveConnections)
                {
                    if (point.Id.CompareTo(connection.Id) > 0)
                    {
                        float actualRating = GetRatingFromDistance((point.Position - _values[connection.Id].Position).Norm);
                        float desiredRating = GetRatingFromDistance(connection.Value);
                        float difference = actualRating - desiredRating;
                        error += difference * difference;
                        numOfConnections++;
                    }
                }
            }
            return (error, numOfConnections);
        }

        public (float, float) GetValError()
        {
            float error = 0;
            int numOfConnections = 0;

            foreach (DataPoint point in _values.Values)
            {
                foreach (Connection connection in point.ActiveConnections)
                {
                    if (point.Id.CompareTo(connection.Id) > 0 && connection.IsForValidation)
                    {
                        float actualRating = GetRatingFromDistance((point.Position - _values[connection.Id].Position).Norm);
                        float desiredRating = GetRatingFromDistance(connection.Value);
                        float difference = actualRating - desiredRating;
                        error += difference * difference;
                        numOfConnections++;
                    }
                }
            }
            return (error, numOfConnections);
        }

        public PointCloud GetCopy()
        {
            Dictionary<string, DataPoint> pointCopies = new Dictionary<string, DataPoint>();
            foreach (KeyValuePair<string, DataPoint> kvp in _values)
            {
                pointCopies.Add(kvp.Key, kvp.Value.GetCopy());
            }
            return new PointCloud(pointCopies, GetRatingFromDistance);
        }

        // New references
        public PointCloud Combine(PointCloud other)
        {
            Dictionary<string, DataPoint> pointCopies = new Dictionary<string, DataPoint>();
            foreach (KeyValuePair<string, DataPoint> kvp in _values)
                pointCopies.Add(kvp.Key, kvp.Value.GetCopy());
            foreach (KeyValuePair<string, DataPoint> kvp in other._values)
                pointCopies.Add(kvp.Key, kvp.Value.GetCopy());
            
            return new PointCloud(pointCopies, GetRatingFromDistance);
        }
        public List<string> Intersect(PointCloud other)
        {
            return _values.Keys.Intersect(other._values.Keys).ToList();
        }

        public List<Vector> GetAllPointPositions()
        {
            List<Vector> acc = new List<Vector>();

            foreach (var (key, val) in _values)
                acc.Add(val.Position);
            
            return acc;
        }

        public PointCloud GetSubCloud(float fraction)
        {
            var original = this.GetCopy()._values.Values.ToList();
            Dictionary<string, DataPoint> selected = new Dictionary<string, DataPoint>();
            var rnd = new Random();

            while (selected.Count < this._values.Count * fraction)
            {
                int index = rnd.Next(0, original.Count);
                selected.Add(original[index].Id, original[index]);
                original.Remove(original[index]);
            }

            return new PointCloud(selected, GetRatingFromDistance);
        }

        public void Remove(DataPoint point)
        {
            _values.Remove(point.Id);
        }

        public Dictionary<string, DataPoint> GetValues()
        {
            return _values;
        }

        // Assumes same rating from distance method
        public static PointCloud Collapse(List<PointCloud> clouds)
        {
            Dictionary<string, DataPoint> pointCopies = new Dictionary<string, DataPoint>();

            foreach (var cloud in clouds)
            {
                foreach (KeyValuePair<string, DataPoint> kvp in cloud.GetValues())
                    pointCopies.Add(kvp.Key, kvp.Value.GetCopy());
            }

            return new PointCloud(pointCopies, clouds[0].GetRatingFromDistance);
        }

        public void SavePointCloudData(string path)
        {
            float[][] positions = new float[GetPointCount()][];
            string[] ids = new string[GetPointCount()];

            for (int i = 0; i < GetPointCount(); i++)
            {
                positions[i] = new float[Dimensions];
                var point = GetValuesAsList()[i];
                for (int j = 0; j < Dimensions; j++)
                    positions[i][j] = point.Position[j];
                ids[i] = point.Id;
            }

            var data = new { Positions = positions, Ids = ids };

            string jsonString = JsonConvert.SerializeObject(data);

            File.WriteAllText(path, jsonString);
        }

        public DataPoint FindPointByID(string id)
        {
            return _values.GetValueOrDefault(id, null);
        }
    }
}

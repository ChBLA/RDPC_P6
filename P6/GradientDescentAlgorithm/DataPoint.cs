using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GradientDescentAlgorithm.Interfaces;
using LinearAlgebra;
using Newtonsoft.Json;

namespace GradientDescentAlgorithm
{
    public class DataPoint : IPoint
    {
        public string Id { get; private set; }
        public Vector Position { get;  set; }
        public Vector Velocity { get; set; }
        public Vector Momentum { get; set; }

        public List<Connection> Connections { get; }
        [JsonIgnore]
        public List<Connection> ActiveConnections { get; set; }
        [JsonIgnore]
        public List<Connection> InactiveConnections
        {
            get => Connections.Except(ActiveConnections).ToList();
        }

        public DataPoint(Vector position, string id)
        {
            Id = id;
            Position = position;
            Connections = new List<Connection>();
            Velocity = new Vector(Position.Length);
            Momentum = new Vector(Position.Length);
        }

        public DataPoint(Vector position, string id, List<Connection> connections): this(position, id)
        {
            Connections = connections;
        }
        
        public DataPoint(List<float> position, string id) : this(new Vector(position.ToArray()), id) { }

        [JsonConstructor]
        public DataPoint(string id, Vector position, Vector velocity, Vector momentum, List<Connection> connections)
        {
            Id = id;
            Position = position;
            Velocity = velocity;
            Momentum = momentum;
            Connections = connections;
        }

        public (Vector, float, float) GetGradientAndErrors(Vector pos, PointCloud points, Func<float, float> getRatingFromDistance)
        {
            Vector gradient = new Vector(Position.Length);
            float error = 0.0f;
            float valError = 0.0f;

            foreach (Connection connection in ActiveConnections)
            {
                Vector diffVector = pos - points[connection.Id].Position;
                float actualDistance = diffVector.Norm;
                float desiredDistance = GetDesiredDistance(connection);
                float differenceOfDistances = getRatingFromDistance(actualDistance) - getRatingFromDistance(desiredDistance);

                if (connection.IsForValidation)
                    valError += differenceOfDistances * differenceOfDistances;
                else
                {
                    error += differenceOfDistances * differenceOfDistances;
                    if (actualDistance != 0.0f)
                        gradient += diffVector * (1 - desiredDistance / actualDistance);
                }
            }

            return (gradient, error, valError);
        }

        public float GetDesiredDistance(Connection connection) => connection.Value;

        public override bool Equals(object obj) => !(obj == null || GetType() != obj.GetType()) && ((DataPoint)obj).Id.Equals(Id);
        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                throw new Exception("Cannot compare IDPoint with non-IDPoint");
            return Id.CompareTo(((DataPoint)obj).Id);
        }

        public DataPoint GetCopy()
        {
            return new DataPoint(Position, Id, Connections);
        }
    }
}

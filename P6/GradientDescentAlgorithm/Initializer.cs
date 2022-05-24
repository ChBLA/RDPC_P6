using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinearAlgebra;

namespace GradientDescentAlgorithm
{
    public class Initializer
    {
        private readonly Dictionary<string, List<(string, float)>> connectionMap; 

        public Initializer(List<(string, string, float)> connections)
        {
            connectionMap = new Dictionary<string, List<(string, float)>>();

            foreach(var entry in connections)
            {
                if (!connectionMap.ContainsKey(entry.Item1))
                    connectionMap.Add(entry.Item1, new List<(string, float)>());
                connectionMap[entry.Item1].Add((entry.Item2, entry.Item3));                    

                if (!connectionMap.ContainsKey(entry.Item2))
                    connectionMap.Add(entry.Item2, new List<(string, float)>());
                connectionMap[entry.Item1].Add((entry.Item1, entry.Item3));
            }
        }

        public List<Vector> GetPositions(int dimensions)
        {
            List<Vector> positions = new List<Vector>();

            string[] points = connectionMap.Keys.ToArray();


            return positions;
        }
    }
}

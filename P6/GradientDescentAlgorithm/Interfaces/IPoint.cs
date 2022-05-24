using System;
using System.Collections.Generic;
using LinearAlgebra;

namespace GradientDescentAlgorithm.Interfaces
{
    public interface IPoint : IComparable
    {
        public Vector Position { get; set; }
        public string Id { get; }
        public List<Connection> Connections { get; }
    }
}

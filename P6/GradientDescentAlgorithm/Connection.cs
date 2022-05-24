using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GradientDescentAlgorithm.Interfaces;

namespace GradientDescentAlgorithm
{
    public class Connection : IConnection
    {
        public string Id { get; }
        public float Value { get; }
        public bool IsForValidation { get; }
        public Connection(string id, float value, bool isForValidation = false )
        {
            Id = id;
            Value = value;
            IsForValidation = isForValidation;
        }
    }
}

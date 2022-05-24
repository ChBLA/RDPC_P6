using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentAlgorithm
{
    public class History
    {
        public float[] Error {get;}
        public float[] ValError { get; }
        public int ConnectionCount { get; }
        public float LR { get; }

        public History(int connectionCount, int iterations, float lr)
        {
            ConnectionCount = connectionCount;
            LR = lr;
            Error = new float[iterations];
            ValError = new float[iterations];
        }


    }
}

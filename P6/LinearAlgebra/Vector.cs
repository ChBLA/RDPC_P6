using System;
using System.Linq;
using Newtonsoft.Json;

namespace LinearAlgebra
{
    public class Vector
    {
        [JsonProperty]
        private float[] _values;

        [JsonIgnore]
        public int Length
        {
            get => _values.Length;
        }
        [JsonIgnore]
        public float NormSqr
        {
            get => Collapse(Length, 0, (float f, int i) => (f + this[i] * this[i]));
        }
        [JsonIgnore]
        public float Norm
        {
            get => (float) Math.Sqrt(NormSqr);
        }

        public Vector GetFirstNEntries(int n)
        {
            return new Vector(_values.Take(n).ToArray());
        }

        public Vector(int size)
        {
            _values = new float[size];
        }

        [JsonConstructor]
        public Vector(float[] values)
        {
            _values = values;
        }

        public float SqrDistanceTo(Vector other)
        {
            return (this - other).NormSqr;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1.Length != v2.Length)
                throw new Exception("Cannot add two vectors of different lengths");
            return Iterate(v1.Length, (int i) => (v1[i] + v2[i]));
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1.Length != v2.Length)
                throw new Exception("Cannot subtract two vectors of different lengths");
            return Iterate(v1.Length, (int i) => (v1[i] - v2[i]));
        }

        public static Vector operator *(Vector v1, float f)
        {
            return Iterate(v1.Length, (int i) => (v1[i] * f));
        }
        public static Vector operator *(float f, Vector v1)
        {
            return Iterate(v1.Length, (int i) => (v1[i] * f));
        }

        public static float operator *(Vector v1, Vector v2)
        {
            if (v1.Length != v2.Length)
                throw new Exception("Cannot take dot product for vectors of different lengths");
            return Collapse(v1.Length, 0, (float f, int i) => (f + v1[i] * v2[i]));
        }


        public static Vector operator /(Vector v1, float f)
        {
            return Iterate(v1.Length, (int i) => (v1[i] / f));
        }


        public static Vector GetUniformVector(int dimensions, int vectorLength)
        {
            Random rnd = new Random();
            return new Vector(new float[dimensions].Select((float f) => ((float)rnd.NextDouble() * vectorLength)).ToArray());
        }

        public float this[int i]
        {
            get
            {
                return _values[i];
            }
        }

        public Vector SquareElements()
        {
            return Iterate(_values.Count(), (int i) => _values[i] * _values[i]);
        }

        public static Vector Adam(Vector momentum, Vector velocity, float learningRate, 
                                  float beta1, float beta2, float epsilon)
        {
            return Iterate(momentum.Length, (int i) => learningRate * Adam(momentum[i] / (1 - beta1), velocity[i] / (1 - beta2), epsilon));
        }

        public static Vector RMSProp(Vector velocity, float learningRate, float epsilon, Vector gradient)
        {
            return Iterate(velocity.Length, (int i) => Adam(learningRate, velocity[i], epsilon) * gradient[i]);
        }

        public float[] ToArray()
        {
            return _values;
        }

        private static float Adam(float biasCorrectedMomentum, float biasCorrectedVelocity, float epsilon)
        {
            return (float) (biasCorrectedMomentum / (Math.Sqrt(biasCorrectedVelocity) + epsilon));
        }

        private static Vector Iterate(int length, Func<int, float> fn)
        {
            float[] res = new float[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = fn(i);
            }
            return new Vector(res);
        }

        private static float Collapse(int length, float startValue, Func<float, int, float> fn)
        {
            float res = startValue;
            for (int i = 0; i < length; i++)
            {
                res = fn(res, i);
            }
            return res;
        }

        public string ToString(int decimals)
        {
            string acc = "{ ";

            for (int i = 0; i < _values.Length; i++)
            {
                acc += string.Format("{0:N" + decimals + "}", _values[i]);
                if (i < _values.Length - 1)
                    acc += "; ";
            }

            return acc + " }";
        }

        public override string ToString()
        {
            return ToString(2);
        }
    }
}

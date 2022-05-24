using GradientDescentAlgorithm.GDOptimisers;
using System;

namespace Settings
{
    public class OptimizerConfig
    {
        public OptimiserType OptimiserType { get; set; }
        public float InitialLearningRate { get; set; }
        public float Momentum { get; set; }
        public bool ProportionalLR { get; set; }
        public float DimScaleLR { get; set; }
        public float DimProportionalityPower { get; set; }
        public DistanceMethod DistanceMethod { get; set; }
        public int VectorDimensions { get; set; }
        public float VectorScale { get; set; }
        public float ValidationSplit { get; set; }
        public bool UseEarlyStopping { get; set; }
        public float RTDPower { get; set; }
        public float RTDConstant { get; set; }
        public int DiffusionDegree { get; set; }
        public float DecayConstant { get; set; }
        public float DecayConstantStep { get; set; }
        public float DecayFactor { get; set; }
        public float DecayFactorStep { get; set; }
        public float DecayPowerBase { get; set; }
        public float DecayPowerBaseStep { get; set; }
        public Func<float, float> GetDistanceMethod()
        {
            switch (DistanceMethod)
            {
                case DistanceMethod.Squared:
                    return (float f) => (float)Math.Pow((6.0f - f), 2.0);
                case DistanceMethod.Power:
                    return (float f) => (float)Math.Pow(7.2 - f, 1.9); //(6 - f) ^ 1.5 has wrong constant term and only works for ml 
                case DistanceMethod.DoubanPower:
                    return (float f) => (float)Math.Pow(6.0 - f, 1.3);
                case DistanceMethod.ML1MPower:
                    return (float f) => (float)Math.Pow(6.4 - f, 1.5);
                default:
                    return (float f) => 6.0f - f;
            }
        }

        public Func<float, float> GetInverseDistanceMethod()
        {
            switch (DistanceMethod)
            {
                case DistanceMethod.Squared:
                    return (float f) => 6 - (float)Math.Sqrt(f);
                case DistanceMethod.Power:
                    return (float f) => 7.2f - (float)Math.Pow(f, 1.0/1.9); //6 - f ^ (1/1.5) has wrong constant term and only works for ml
                case DistanceMethod.DoubanPower:
                    return (float f) => 6.0f - (float)Math.Pow(f, 1.0 / 1.3);
                case DistanceMethod.ML1MPower:
                    return (float f) => 6.4f - (float)Math.Pow(f, 1.0 / 1.5);
                default:
                    return (float f) => 6.0f - f;
            }
        }
        public float LearningRate
        {
            get => InitialLearningRate + VectorDimensions * DimScaleLR;
        }

    }
    
    public enum DistanceMethod
    {
        Flipped,
        Squared,
        Power,
        DoubanPower,
        ML1MPower
    }
}
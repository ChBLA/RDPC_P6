using GradientDescentAlgorithm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentAlgorithm.GDOptimisers
{
    public enum OptimiserType
    {
        Adam,
        RMSProp,
        Nesterov,
        Momentum,
        Vanilla
    }
    
    public static class Entensions
    {
        public static IGradientDescentStrategy GetOptimiser(this OptimiserType type)
        {
            switch (type)
            {
                case OptimiserType.Adam: return new AdamOptimiser();
                case OptimiserType.Momentum: return new MomentumOptimiser();
                case OptimiserType.Nesterov: return new NesterovOptimiser();
                case OptimiserType.RMSProp: return new RMSPropOptimiser();
                default: return new VanillaOptimiser();
            }
        } 

        public static string GetName(this OptimiserType type)
        {
            switch (type)
            {
                case OptimiserType.Adam: return "adam";
                case OptimiserType.RMSProp: return "rmsprop";
                case OptimiserType.Nesterov: return "nesterov";
                case OptimiserType.Momentum: return "momentum";
                case OptimiserType.Vanilla: return "vanilla";
                default: return "unknown";
            }
        }
    }
    

}

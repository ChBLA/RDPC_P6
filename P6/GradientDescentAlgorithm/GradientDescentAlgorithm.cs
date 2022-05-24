using GradientDescentAlgorithm.Interfaces;
using System;
using System.Collections.Generic;
using NLog;
using NLog.Fluent;

namespace GradientDescentAlgorithm
{
    public class GradientDescentAlgorithm
    {
        private static readonly int errorPlotHeight = 25, errorPlotWidth = 100;
        private static readonly Logger Logger = LogManager.GetLogger(nameof(GradientDescentAlgorithm));

        public static History Run(PointCloud points, float learningRate, int iterations, 
                                    Func<float, float> getRatingFromDistance, IGradientDescentStrategy gdStrategy, 
                                    bool graphError = false, float validationSplit = -1, bool noPrint = false, 
                                    float minImprovement = -25.0f, bool enableEarlyStopping = false)
        {
            float initialError = 0;
            float scale = 0f;
            int step = 0;
            int noImprovementCounter = 0;
            float patience = 125.0f;
            bool earlyStop = false;
            float bestError = float.MaxValue;
            History history = new History(points.GetActiveRatingsCount(), iterations, learningRate);
            for (int i = 0; i < iterations && !earlyStop; i++)
            {
                history.Error[i] = 0;
                foreach (DataPoint point in points)
                {
                    (float error, float valError) = gdStrategy.UpdatePoint(points, point, learningRate, getRatingFromDistance);
                    history.Error[i] += error;
                    history.ValError[i] += valError;
                }

                history.Error[i] *= 0.5f;
                history.ValError[i] *= 0.5f;

                if (graphError && !noPrint)
                {
                    if (initialError == 0)
                    {
                        initialError = history.Error[i];
                        scale = (float)(errorPlotHeight / Math.Log2(initialError));
                    }

                    while (errorPlotHeight - Math.Log2(history.Error[i]) * scale > step)
                    {
                        PrintErrorPlot(history.Error[i], initialError, i, iterations);
                        step++;
                    }
                }
                if (!noPrint)
                {
                    string validationError = validationSplit > 0 ? $"validation MSE: " +
                                             $"{ FormatNumber(history.ValError[i] / validationSplit / points.GetActiveRatingsCount())}" : "";
                    Logger.Info($"{i}/{iterations}: MSE: " +
                                  $"{FormatNumber(history.Error[i] / (1f - validationSplit) / points.GetActiveRatingsCount())}, " +
                                  $"{validationError} {new String(' ', 30)}");
                }

                // No improvement
                if (enableEarlyStopping)
                {
                    if (((validationSplit > 0 ? history.ValError[i] : history.Error[i]) - bestError) >= minImprovement)
                        noImprovementCounter++;
                    else
                    {
                        bestError = (validationSplit > 0 ? history.ValError[i] : history.Error[i]);
                        noImprovementCounter = 0;
                    }

                    if (noImprovementCounter >= (int)patience)
                        earlyStop = true;

                    patience = patience * 0.95f + 0.25f;
                }
            }

            if (graphError && !noPrint) Logger.Info($"{new String('-', errorPlotWidth)}");

            return history;
        }

        private static string FormatNumber(float f)
        {
            return String.Format("{0:n}", f);
        }

        private static void PrintErrorPlot(float error, float initialError, int iteration, int totalIterations)
        {
            Console.WriteLine($"\r|█{new String('█', iteration * errorPlotWidth / totalIterations)} " +
                              $"{FormatNumber(error / initialError * 100)}%{new String(' ', 60)}");
        }
    }
}

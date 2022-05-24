using Experiments.Interfaces;
using Experiments.Tools;
using GradientDescentAlgorithm.GDOptimisers;
using IdentifiablePoints;
using Newtonsoft.Json;
using PythonBindings;
using Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Experiments.DiffusionExperiment;

namespace Experiments
{
    public class Tuning
    {
        private PointLoader _loader;
        private AppConfig _appConfig;
        private OptimizerConfig _optimizationConfig;

        public Tuning(PointLoader loader, AppConfig appConfig, OptimizerConfig optimizationConfig)
        {
            _loader = loader;
            _appConfig = appConfig;
            _optimizationConfig = optimizationConfig;
        }

        public (IExperimentResult, Action) TuneDimensions()
        {
            (int, int) interval = _appConfig.DataType.Equals(DataType.Douban) ? (2, 70) : (2, 20);
            var dimResults = new PointCloudExperiments(_loader, _optimizationConfig).VaryDimensionAndLearningRate(
                interval, 
                (int a) => _optimizationConfig.InitialLearningRate * (0.5f + 0.25f * a), 
                5, 
                _optimizationConfig.OptimiserType, 
                iterations: _appConfig.Iterations
            );

            Action pythonScript = () =>
            {
                var arguments = new List<string>() 
                { 
                    "-q", "--val_split", 
                    _optimizationConfig.ValidationSplit.ToString(new System.Globalization.CultureInfo("en-GB")), 
                    "--file" 
                };
                arguments.Add(_appConfig.HistorySaveFilePath);
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("dimensions.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (new Tuning1(dimResults), pythonScript);
        }

        public (IExperimentResult, Action) TuneRTDConstant()
        {
            float constInitVal = 5.0f;
            float constStep = 0.2f;
            var results = new PointCloudExperiments(_loader).VaryRTDConstant(
                (int a) => _optimizationConfig.InitialLearningRate * (0.5f + 0.25f * a),
                5,
                _optimizationConfig.RTDPower,
                (int a) => (constInitVal + constStep * a),
                20,
                _optimizationConfig.OptimiserType,
                _optimizationConfig.VectorDimensions,
                _appConfig.Iterations
            );

            Action pythonScript = () =>
            {
                var arguments = new List<string>()
                {
                    "-q", "--func", "constant", 
                    "--val_split", _optimizationConfig.ValidationSplit.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--c_init", constInitVal.ToString(new System.Globalization.CultureInfo("en-GB")), 
                    "--c_step", constStep.ToString(new System.Globalization.CultureInfo("en-GB")), 
                    "--file" 
                };
                arguments.Add(_appConfig.HistorySaveFilePath);
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("plotRTD.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (new Tuning1(results), pythonScript);
        }

        public (IExperimentResult, Action) TuneRTDPower()
        {
            float powerInitVal = 0.75f;
            float powerStep = 0.1f;
            var results = new PointCloudExperiments(_loader).VaryRTDPower(
                (int a) => _optimizationConfig.InitialLearningRate * (0.5f + 0.25f * a),
                5,
                _optimizationConfig.RTDConstant,
                (int a) => (powerInitVal + powerStep * a),
                20,
                _optimizationConfig.OptimiserType,
                _optimizationConfig.VectorDimensions,
                _appConfig.Iterations
            );

            Action pythonScript = () =>
            {
                var arguments = new List<string>()
                {
                    "-q", "--func", "power", 
                    "--val_split", _optimizationConfig.ValidationSplit.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--p_init", powerInitVal.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--p_step", powerStep.ToString(new System.Globalization.CultureInfo("en-GB")), 
                    "--file"
                };
                arguments.Add(_appConfig.HistorySaveFilePath);
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("plotRTD.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (new Tuning1(results), pythonScript);
        }

        // From 10% to 200% of best lr based on previous experiments
        public (Dictionary<string, IExperimentResult>, Action) TuneOptimisers()
        {
            var results = new Dictionary<string, IExperimentResult>();
            var optimisers = new OptimiserType[] { OptimiserType.Adam, OptimiserType.RMSProp, OptimiserType.Nesterov, OptimiserType.Momentum, OptimiserType.Vanilla };
            var lr = new float[] { 2.8f, 1.2f, 0.00052f, 0.00085f, 0.00085f };
            var dn_lr = new float[] { 0.364f, 1.72f, 0.00168f, 0.001f, 0.02494f };

            for (int i = 0; i < optimisers.Length; i++)
            {
                float bestLR = _appConfig.DataType.Equals(DataType.Douban) ? dn_lr[i] : lr[i];
                Func<int, float> lrFunc = (int h) => (0.1f + 0.1f * h) * bestLR;
                Console.WriteLine("Optimiser: " + Enum.GetName(optimisers[i]) + $" with base lr = {bestLR}");
                var imRes = new PointCloudExperiments(_loader, _optimizationConfig).VaryLearningRate(
                    lrFunc, 20, optimisers[i], _optimizationConfig.VectorDimensions, _appConfig.Iterations
                );
                results.Add(optimisers[i].GetName(), new Tuning2(imRes));
            }

            Action pythonScript = () =>
            {
                var arguments = new List<string>()
                {
                    "-q", "-u",
                    "--adam_lr", (_appConfig.DataType.Equals(DataType.Douban) ? dn_lr[0] : lr[0]).ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--rmsprop_lr", (_appConfig.DataType.Equals(DataType.Douban) ? dn_lr[1] : lr[1]).ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--nesterov_lr", (_appConfig.DataType.Equals(DataType.Douban) ? dn_lr[2] : lr[2]).ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--momentum_lr", (_appConfig.DataType.Equals(DataType.Douban) ? dn_lr[3] : lr[3]).ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--vanilla_lr", (_appConfig.DataType.Equals(DataType.Douban) ? dn_lr[4] : lr[4]).ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--adam_f", _appConfig.HistorySaveFilePathNoExt + optimisers[0].GetName() + _appConfig.FileExtensions,
                    "--rmsprop_f", _appConfig.HistorySaveFilePathNoExt + optimisers[1].GetName() + _appConfig.FileExtensions,
                    "--nesterov_f", _appConfig.HistorySaveFilePathNoExt + optimisers[2].GetName() + _appConfig.FileExtensions,
                    "--momentum_f", _appConfig.HistorySaveFilePathNoExt + optimisers[3].GetName() + _appConfig.FileExtensions,
                    "--vanilla_f", _appConfig.HistorySaveFilePathNoExt + optimisers[4].GetName() + _appConfig.FileExtensions,
                };
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("OptimiserPlots.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (results, pythonScript);
        }

        public (IExperimentResult, Action) TuneDiffusionLRAndDecay()
        {
            var depth = 15;
            var exploration = 15;
            var results = new DiffusionExperiment(_loader, _optimizationConfig, _appConfig).ContinuousSmallDiffusion(depth, exploration);

            Action pythonScript = () =>
            {
                var arguments = new List<string>()
                {
                    "-q",
                    "--constant", _optimizationConfig.DecayConstant.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--files", _appConfig.HistorySaveFilePath
                };
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("ContinuousSmallDiffusionExperiment.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (new Tuning3(results), pythonScript);
        }

        public (IExperimentResult, Action) TuneDecayConstant()
        {
            var results = new ConcurrentDictionary<int, Experiment2Rapport>();
            var exploration = 30;
            var depth = 15;

            Func<int, Func<int, float>> lrFuncGenerator = (int i) =>
                ((int s) => (_optimizationConfig.DecayConstant + _optimizationConfig.DecayConstantStep * i) + _optimizationConfig.DecayFactor * (float)Math.Pow(_optimizationConfig.DecayPowerBase, s));

            var Providers = Enumerable.Range(0, exploration);
            // Optimise concurrently + time measurement
            Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
            {
                var sTIme = DateTime.Now;
                results.AddOrUpdate(currentProvider, new DiffusionExperiment(_loader, _optimizationConfig, _appConfig)
                    .SimpleDiffusionRun((int i) => 50,
                    lrFuncGenerator(currentProvider), 0.0f, _optimizationConfig.DiffusionDegree, depth).Item1, (key, oldValue) => oldValue);
                Console.WriteLine($"Thread {currentProvider} stopped after {(DateTime.Now - sTIme).TotalSeconds}");
            });

            Action pythonScript = () =>
            {
                var arguments = new List<string>()
                {
                    "-q",
                    "--type", "c",
                    "--factor", _optimizationConfig.DecayFactor.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--base", _optimizationConfig.DecayPowerBase.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--files", _appConfig.HistorySaveFilePath
                };
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("ContinuousSmallDiffusionExperiment.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (new Tuning4(results), pythonScript);
        }

        public (IExperimentResult, Action) TuneDecayFactor()
        {
            var results = new ConcurrentDictionary<int, Experiment2Rapport>();
            var exploration = 15;
            var depth = 15;

            Func<int, Func<int, float>> lrFuncGenerator = (int i) =>
                ((int s) => _optimizationConfig.DecayConstant + (_optimizationConfig.DecayFactor + i * _optimizationConfig.DecayFactorStep) * (float)Math.Pow(_optimizationConfig.DecayPowerBase, s));

            var Providers = Enumerable.Range(0, exploration);
            // Optimise concurrently + time measurement
            Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
            {
                var sTIme = DateTime.Now;
                results.AddOrUpdate(currentProvider, new DiffusionExperiment(_loader, _optimizationConfig, _appConfig)
                    .SimpleDiffusionRun((int i) => 50,
                    lrFuncGenerator(currentProvider), 0.0f, _optimizationConfig.DiffusionDegree, depth).Item1, (key, oldValue) => oldValue);
                Console.WriteLine($"Thread {currentProvider} stopped after {(DateTime.Now - sTIme).TotalSeconds}");
            });

            Action pythonScript = () =>
            {
                var arguments = new List<string>()
                {
                    "-q",
                    "--type", "f",
                    "--constant", _optimizationConfig.DecayConstant.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--base", _optimizationConfig.DecayPowerBase.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--files", _appConfig.HistorySaveFilePath
                };
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("ContinuousSmallDiffusionExperiment.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (new Tuning4(results), pythonScript);
        }

        public (IExperimentResult, Action) TuneDecayBase()
        {
            var results = new ConcurrentDictionary<int, Experiment2Rapport>();
            var exploration = 15;
            var depth = 15;

            Func<int, Func<int, float>> lrFuncGenerator = (int i) =>
                ((int s) => _optimizationConfig.DecayConstant + _optimizationConfig.DecayFactor * (float)Math.Pow(_optimizationConfig.DecayPowerBase + i * _optimizationConfig.DecayPowerBaseStep, s));

            var Providers = Enumerable.Range(0, exploration);
            // Optimise concurrently + time measurement
            Parallel.ForEach(Providers, currentProvider => //new ParallelOptions { MaxDegreeOfParallelism = 3 },
            {
                var sTIme = DateTime.Now;
                results.AddOrUpdate(currentProvider, new DiffusionExperiment(_loader, _optimizationConfig, _appConfig)
                    .SimpleDiffusionRun((int i) => 50,
                    lrFuncGenerator(currentProvider), 0.0f, _optimizationConfig.DiffusionDegree, depth).Item1, (key, oldValue) => oldValue);
                Console.WriteLine($"Thread {currentProvider} stopped after {(DateTime.Now - sTIme).TotalSeconds}");
            });

            Action pythonScript = () =>
            {
                var arguments = new List<string>()
                {
                    "-q",
                    "--type", "b",
                    "--factor", _optimizationConfig.DecayFactor.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--constant", _optimizationConfig.DecayConstant.ToString(new System.Globalization.CultureInfo("en-GB")),
                    "--files", _appConfig.HistorySaveFilePath
                };
                PythonCaller pyCaller = new PythonCaller();
                var res = pyCaller.ExecutePythonFile("ContinuousSmallDiffusionExperiment.py", new string[] { "DataVisualisation" }, arguments);
                Console.WriteLine(res);
            };

            return (new Tuning4(results), pythonScript);
        }

        private class Tuning1 : IExperimentResult
        {
            private Dictionary<int, ConcurrentDictionary<float, RunData>> results;
            public Tuning1(Dictionary<int, ConcurrentDictionary<float, RunData>> results)
            {
                this.results = results;
            }
            public string SerializeResult() => JsonConvert.SerializeObject(results);

        }

        private class Tuning2 : IExperimentResult
        {
            private ConcurrentDictionary<float, RunData> results;

            public Tuning2(ConcurrentDictionary<float, RunData> results)
            {
                this.results = results;
            }
            public string SerializeResult() => JsonConvert.SerializeObject(results);
        }

        private class Tuning3 : IExperimentResult
        {
            private Dictionary<int, ConcurrentDictionary<int, Experiment2Rapport>> results;

            public Tuning3(Dictionary<int, ConcurrentDictionary<int, Experiment2Rapport>> results)
            {
                this.results = results;
            }
            public string SerializeResult() => JsonConvert.SerializeObject(results);
        }

        private class Tuning4 : IExperimentResult
        {
            private ConcurrentDictionary<int, Experiment2Rapport> results;

            public Tuning4(ConcurrentDictionary<int, Experiment2Rapport> results)
            {
                this.results = results;
            }
            public string SerializeResult() => JsonConvert.SerializeObject(results);
        }
    }
}

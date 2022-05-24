using System;
using System.IO;
using Newtonsoft.Json;
using Utils;

namespace Settings
{
    public class AppConfig
    {
        public AppConfig()
        {
            PathHandler pathHandler = new PathHandler();
            _separator = pathHandler.Separator;
            _code_dir = pathHandler.CodeDir;
        }

        public RunMode RunMode { get; set; }
        public ExperimentType ExperimentType { get; set; }
        public int Iterations { get; set; }
        public string SaveFileName { get; set; }
        public string HistoryFileName { get; set; }
        public string DataFileName { get; set; }
        public string MetaDataFileName { get; set; }
        public DataType DataType { get; set; }
        public string DataFolderName { get; set; }
        public string InputDataFolder { get; set; }
        public string OutputHistoryFolder { get; set; }
        public string OutputPointCloudFolder { get; set; }
        public string FileExtensions { get; set; }
        public bool Verbose { get; set; }
        public bool SaveOutput { get; set; }
        public bool SaveResults { get; set; }
        public string PythonScriptsDirName { get; set; }

        private readonly string _separator;
        private readonly string _code_dir;

         public string DataDirPath
         {
             get => _code_dir + _separator + DataFolderName;
         }       
        
        public string SaveFilePath
        {
            get => DataDirPath + _separator + OutputPointCloudFolder  + _separator + SaveFileName + FileExtensions;
        }

        public string SaveFilePathNoExt
        {
            get => DataDirPath + _separator + OutputPointCloudFolder + _separator + SaveFileName;
        }

        public string DataFilePath
        {
            get => DataDirPath + _separator + InputDataFolder + _separator + DataFileName + FileExtensions;
        }

        public string MetaDataFilePath
        {
            get => DataDirPath + _separator + InputDataFolder + _separator + MetaDataFileName + FileExtensions;
        }

        public string HistoryFolderPath
        {
            get => DataDirPath + _separator + OutputHistoryFolder;
        }
        
        public string HistorySaveFilePathNoExt
        {
            get => HistoryFolderPath +  _separator + HistoryFileName;
        }
        
        public string HistorySaveFilePath
        {
            get => HistorySaveFilePathNoExt + FileExtensions;
        }
        
    }

    public enum RunMode
    {
        Normal,
        Testing,
        DimTesting,
        LRTesting,
        LRDimMapping,
        LRPointCloudSizeMapping,
        MultiRun
    }

    public enum ExperimentType
    {
        VanillaBenchmark,
        DiffusionBenchmark,
        TuneLearningRate,
        TuneDimensions,
        TuneRTDFunction,
        TuneRTDConstant,
        TuneRTDPower,
        TuneOptimisers,
        TrueDiffusion,
        TuneLRAndDecay,
        TuneLRDecayConstant,
        TuneLRDecayFactor,
        TuneLRDecayBase
    }

    public enum DataType
    {
        ML1M,
        ML10M,
        Douban,
        MovieLens
    }
}
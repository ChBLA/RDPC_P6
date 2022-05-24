using System.IO;
using Newtonsoft.Json;

namespace Settings
{
    public class ConfigManager
    {
        public ConfigManager(Modes mode)
        {
            string relativePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            
            string optConfFilePath = File.ReadAllText(relativePath + @"/Settings/OptimizerConfig.json");
            OptimizerConfig = JsonConvert.DeserializeObject<OptimizerConfig>(optConfFilePath);
            
            string appConfFilePath = File.ReadAllText(relativePath + @"/Settings/AppConfig.json");
            AppConfig = JsonConvert.DeserializeObject<AppConfig>(appConfFilePath);
        }

        public OptimizerConfig OptimizerConfig;
        public AppConfig AppConfig;

    }
    
    public enum Modes
    {
        Debug,
        Experimental,
        Training
    }
}
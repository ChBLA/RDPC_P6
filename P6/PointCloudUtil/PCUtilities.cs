using GradientDescentAlgorithm;
using Newtonsoft.Json;
using Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudUtil
{
    public class PCUtilities
    {
        static public void SavePointCloudData(PointCloud cloud, AppConfig appConfig, OptimizerConfig optimizerConfig, int number = -1)
        {
            float[][] positions = new float[cloud.GetPointCount()][];
            string[] ids = new string[cloud.GetPointCount()];

            for (int i = 0; i < cloud.GetPointCount(); i++)
            {
                positions[i] = new float[optimizerConfig.VectorDimensions];
                var point = cloud.GetValuesAsList()[i];
                for (int j = 0; j < optimizerConfig.VectorDimensions; j++)
                    positions[i][j] = point.Position[j];
                ids[i] = point.Id;
            }

            var data = new { Positions = positions, Ids = ids };

            string jsonString = JsonConvert.SerializeObject(data);

            File.WriteAllText(number == -1 ? appConfig.SaveFilePath : appConfig.SaveFilePathNoExt + number + appConfig.FileExtensions, jsonString);
        }
    }
}

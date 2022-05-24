using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using GradientDescentAlgorithm;
using NLog;

namespace IdentifiablePoints
{
    public class PointLoader
    {
        private static readonly Logger Logger = LogManager.GetLogger(nameof(PointLoader));

        public List<(string, string, float, int)> Connections { get; }

        public PointLoader(string filePath)
        {
            Connections = new List<(string, string, float, int)>();
            try
            {
                string text = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<DataContainer[]>(text);

                foreach (var entry in data)
                    Connections.Add((entry.FirstId, entry.SecondId, entry.rating, entry.val));
            }
            catch (OutOfMemoryException)
            {
                using (var reader = File.OpenText(filePath))
                {
                    string line;
                    string buffer = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!(line.Contains("[") || line.Contains("]")))
                            buffer += line;
                        if (line.Contains("},"))
                        {
                            var formattedLine = buffer.Replace("\n", " ").Substring(0, buffer.Length - 1);
                            var dataLine = JsonConvert.DeserializeObject<DataContainer>(formattedLine);
                            Connections.Add((dataLine.FirstId, dataLine.SecondId, dataLine.rating, dataLine.val));
                            // Console.WriteLine($"({dataLine.FirstId}, {dataLine.SecondId}, {dataLine.rating}");
                            buffer = "";
                        }

                    }
                }
            }
        }

        // Creates files matching the parameter, and return the path to the folder in which all subfiles are located
        public static (string, Dictionary<string, int>) DiffusedFileGeneration(string ratingFilePath, string metaFilePath, int numOfFiles, float validationSplit)
        {
            string folderPath = Path.Combine(Path.Combine(ratingFilePath, ".."), "subclouds");
            CreateFolder(folderPath);
            
            var files = CreateFiles(folderPath, numOfFiles); // data will be "distributed" into these files
            
            AppendToAll(files, "[");

            // Load all points (by metaFilePath) and assign to files
            var pointMap = AssignPointsToFiles(metaFilePath, files);
            
            // Iterate through rating file in chunks
            ProcessRatingsFile(ratingFilePath, files, pointMap, validationSplit);

            Finalize(files);
            return (folderPath, pointMap); 
        }

        private static void CreateFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Logger.Info($"DELETING {folderPath} in 10 seconds");
                Thread.Sleep(10000);
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
        }

        private static string[] CreateFiles(string folderPath, int numOfFiles)
        {
            var files = new string[numOfFiles];
            for (int i = 0; i < numOfFiles; i++)
            {
                files[i] = Path.Combine(folderPath, $"rsc_{i}.json");
                File.Create(files[i]).Close();
            }
            return files;
        }

        private static void AppendToAll(string[] files, string s)
        {
            for (int i = 0; i < files.Length; i++)
            {
                var file = File.AppendText(files[i]);
                file.WriteLine("[");
                file.Close();
            }
        }
        
        private static Dictionary<string, int> AssignPointsToFiles(string metaDataFilePath, string[] files)
        {
            var rnd = new Random();
            var points = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(metaDataFilePath)).ToList();
            var pointMap = new Dictionary<string, int>();
            while (points.Count != 0)
            {
                for (int i = 0; i < files.Length && points.Count > 0; i++)
                {
                    var randomPoint = points[rnd.Next(0, points.Count)];
                    pointMap.Add(randomPoint, i); 
                    points.Remove(randomPoint);
                }
            }
            return pointMap;
        }
        
        private static void ProcessRatingsFile(string ratingFilePath, string[] files, 
                                                Dictionary<string, int> pointMap, float validationSplit)
        {
            var rnd = new Random();
            
            // Determine chunk size (flat 1 mil)
            int chunkSize = 100000;

            using (var reader = File.OpenText(ratingFilePath))
            {
                string line;
                string buffer = "";
                while (!reader.EndOfStream)
                {
                    int counter = 0;

                    // Create temp list
                    var lists = new List<string>[files.Length];
                    for (int i = 0; i < lists.Length; i++)
                        lists[i] = new List<string>();

                    while (counter < chunkSize && (line = reader.ReadLine()) != null)
                    {
                        if (!(line.Contains("[") || line.Contains("]")))
                            buffer += line;
                        if (line.Contains("},"))
                        {
                            var formattedLine = buffer.Replace("\n", " ").Substring(0, buffer.Length - 1);
                            var dataLine = JsonConvert.DeserializeObject<DataContainer>(formattedLine);
                            string insertString = buffer.Replace(" ", "");
                            int isForValidation = rnd.NextDouble() > validationSplit ? 0 : 1;
                            insertString = insertString.Insert(insertString.Length - 3,
                                $"\",\"val\":\"{isForValidation}");
                            lists[pointMap[dataLine.FirstId]].Add(insertString);
                            if (!dataLine.FirstId.Equals(dataLine.SecondId))
                                lists[pointMap[dataLine.SecondId]].Add(insertString);
                            // Logger.Info($"({dataLine.FirstId}, {dataLine.SecondId}, {dataLine.rating}");
                            buffer = "";
                            counter++;
                        }
                    }

                    // Append to files
                    // insert data into the files
                    for (int i = 0; i < files.Length; i++)
                        File.AppendAllLines(files[i], lists[i]);
                }
            }
        }

        private static void Finalize(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                var file = File.AppendText(files[i]);
                file.WriteLine("]");
                file.Close();
            }
        }
        
        public static PointCloud LoadPointCloudFromFile(string filePath, Func<float, float> ratingDistanceMethod)
        {
            var data = JsonConvert.DeserializeObject<DataPoint[]>(File.ReadAllText(filePath));
            var points = new Dictionary<string, DataPoint>();

            foreach (var point in data)
                points.Add(point.Id, point);

            return new PointCloud(points, ratingDistanceMethod);
        }

        public static void SavePointCloudToFile(string filePath, PointCloud cloud)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(cloud, Formatting.Indented));
        }

    }

    public class DataContainer
    {
        // For the first identifier
        public string user_id;
        public string userid;

        // For the second identifier
        public string book_id;
        public string bookid;
        public string movie_id;
        public string movieid;

        // For rating
        public float rating;

        public int val = -1;

        public DataContainer()
        {
        }

        public string FirstId { get => user_id ?? userid; }
        public string SecondId { get => book_id ?? bookid ?? movie_id ?? movieid; }
    }
}

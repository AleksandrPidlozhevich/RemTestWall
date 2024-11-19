using Newtonsoft.Json;
using System;
using System.IO;

namespace RemTestWall.Services
{
    public static class JsonService
    {
        private static string jsonFileName = typeof(JsonService).Namespace;
        private static string applicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string JsonFolderPath = $@"{applicationPath}\RemTestWall_Data\RemTestWall_Json\{jsonFileName}";
        public static string JsonFilePath = $@"{JsonFolderPath}\{jsonFileName}.json";

        static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            CheckAdditionalContent = true,
            Formatting = Formatting.Indented
        };

        public static void Write(object objectToSerialize)
        {
            try
            {
                if (!Directory.Exists(JsonFolderPath))
                {
                    Directory.CreateDirectory(JsonFolderPath);
                }
                using (StreamWriter writer = File.CreateText(JsonFilePath))
                {
                    string output = JsonConvert.SerializeObject(objectToSerialize);
                    writer.Write(output);
                }
            }
            catch (Exception ex)
            {
                LogService.Error($"Write error.\n{ex.Message}");
            }
        }

        public static void WriteAsync(object objectToSerialize)
        {
            try
            {
                if (!Directory.Exists(JsonFolderPath))
                {
                    Directory.CreateDirectory(JsonFolderPath);
                }
                using (StreamWriter writer = File.CreateText(JsonFilePath))
                {
                    string output = JsonConvert.SerializeObject(objectToSerialize);
                    writer.WriteAsync(output);
                }
            }
            catch (Exception ex)
            {
                LogService.Error($"Write error.\n{ex.Message}");
            }
        }

        public static T Deserialize<T>(string json) where T : new()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception ex)
            {
                LogService.Error($"{nameof(Deserialize)} error.\n{ex.Message}");
                return default(T);
            }
        }

        public static T Read<T>() where T : new()
        {
            T result = default(T);
            try
            {
                if (File.Exists(JsonFilePath))
                {
                    using (var reader = File.OpenText(JsonFilePath))
                    {
                        string fileText = reader.ReadToEnd();
                        var deserializedObject = Deserialize<T>(fileText);
                        if (deserializedObject != null)
                        {
                            result = deserializedObject;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.Error($"{nameof(Read)} error.\n{ex.Message}");
            }

            return result;
        }

        public static bool FileExists()
        {
            return File.Exists(JsonFilePath);
        }
    }
}

using CtATracker.skills;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtATracker
{
    internal class FileHandler
    {
        const string filePath = "data.txt";

        public static List<SkillHandler.SkillConfig> LoadData()
        {
            try
            {
                File.ReadAllText(filePath);
                YamlDotNet.Serialization.Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
                string yamlContent = File.ReadAllText(filePath);
                List<SkillHandler.SkillConfig> data = deserializer.Deserialize<List<SkillHandler.SkillConfig>>(yamlContent);
                return data;
            }
            catch (FileNotFoundException)
            {
                // If the file does not exist, return an empty list
                return new List<SkillHandler.SkillConfig>();
            }
            catch (Exception ex)
            {
                // Handle other exceptions (e.g., deserialization errors)
                Console.WriteLine($"Error loading data: {ex.Message}");
                return new List<SkillHandler.SkillConfig>();
            }
        }


        public static void SaveData(List<SkillHandler.SkillConfig> data)
        {
            YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.Serializer();
            string yamlRepr = serializer.Serialize(data);
            File.WriteAllText(filePath, yamlRepr);
        }
    }
}

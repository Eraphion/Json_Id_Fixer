using Newtonsoft.Json;
using System.IO;

namespace JSONEditor
{

    class IdGenerator
    {
        private long _id;
        public IdGenerator()
        {
            _id = 1;
        }
        public void Reset()
        {
            _id = 1;
        }
        public long Next()
        {
            return _id++;
        }
    }

    class Program
    {
        static readonly string ID = "\"id\"";
        static readonly string outputFolderName = "fixed";
        static readonly string inputFolderName = "input";


        static void ParseJson(dynamic records, IdGenerator gen, long? parentId)
        {
            foreach (var record in records)
            {
                long? fixedId = null;
                record.parent_id = parentId;
                foreach (var key in record)
                {
                    string keyAsString = key.ToString();
                    foreach (var field in key)
                    {
                        if (field.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                        {
                            ParseJson(field, gen, fixedId);
                        }
                        else
                        {
                            if (keyAsString.StartsWith(ID))
                            {
                                fixedId = gen.Next();
                                field.Value = fixedId;
                            }
                        }
                    }
                }
            }
        }

        static void ProcessSingleJSON(string JsonName, string JsonOutput)
        {
            string JsonAsStrings = File.ReadAllText(JsonName);
            dynamic json = JsonConvert.DeserializeObject(JsonAsStrings);

            var gen = new IdGenerator();
            ParseJson(json, gen, null);

            var toJson = JsonConvert.SerializeObject(json);
            Directory.CreateDirectory(outputFolderName);
            File.WriteAllText(JsonOutput, toJson);
        }


        static void Main(string[] args)
        {
            DirectoryInfo dir = new DirectoryInfo(inputFolderName);
            FileInfo[] files = dir.GetFiles("*.json");
            foreach (var i in files)
                ProcessSingleJSON($"{inputFolderName}\\{i.Name}", $"{outputFolderName}\\{i.Name}");
        }
    }
}

using AltV.Net;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PedSyncer.Utils
{
    class FileControl
    {
        public static TDumpType LoadDataFromDumpFile<TDumpType>(string dumpFileName)
            where TDumpType : new()
        {
            TDumpType dumpResult = default;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dumpFileName);
            Alt.Log("Search File " + filePath);
            if (!File.Exists(filePath))
            {
                Alt.Log($"Could not find dump file at {filePath}");
                return default;
            }

            try
            {
                MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                dumpResult = MessagePackSerializer.Deserialize<TDumpType>(File.ReadAllBytes(filePath), lz4Options);
                Alt.Log($"Successfully loaded dump file {dumpFileName}.");
            }
            catch (Exception e)
            {
                Alt.Log($"Failed loading dump: {e}");
            }

            return dumpResult;
        }
        public static TDumpType LoadDataFromJsonFile<TDumpType>(string dumpFileName)
            where TDumpType : new()
        {
            TDumpType dumpResult = default;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dumpFileName);
            Alt.Log("Search File " + filePath);
            if (!File.Exists(filePath))
            {
                Alt.Log($"Could not find dump file at {filePath}");
                return default;
            }

            try
            {
                dumpResult = JsonConvert.DeserializeObject<TDumpType>(File.ReadAllText(filePath));
                Console.WriteLine($"Successfully loaded dump file {dumpFileName}.");
            }
            catch (Exception e)
            {
                Alt.Log($"Failed loading dump: {e}");
            }

            return dumpResult;
        }
    }
}

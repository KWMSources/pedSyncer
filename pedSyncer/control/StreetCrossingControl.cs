using AltV.Net;
using Newtonsoft.Json;
using pedSyncer.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace pedSyncer.control
{
    class StreetCrossingControl
    {
        public static Dictionary<string, StreetCrossing> MappedStreetCrossings = new Dictionary<string, StreetCrossing>();

        //Singleton
        private static StreetCrossingControl instance = null;

        private StreetCrossingControl()
        {
            //Load the pre-calculated StreetCrossings
            List<StreetCrossing> streetCrossings = LoadDataFromJsonFile<List<StreetCrossing>>("resources/pedSyncer2/StreetCrossings.json");
            Alt.Log("StreetCrossings Count: " + streetCrossings.Count);

            foreach(StreetCrossing streetCrossing in streetCrossings)
            {
                MappedStreetCrossings.Add(streetCrossing.Position.ToString(), streetCrossing);
            }
        }

        public static StreetCrossingControl getInstance()
        {
            if (instance == null) instance = new StreetCrossingControl();
            return instance;
        }

        /**
         * Support-Methods
         */
        static TDumpType LoadDataFromJsonFile<TDumpType>(string dumpFileName)
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

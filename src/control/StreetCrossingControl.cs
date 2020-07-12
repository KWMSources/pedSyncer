using AltV.Net;
using Newtonsoft.Json;
using PedSyncer.Model;
using PedSyncer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PedSyncer.Control
{
    class StreetCrossingControl
    {
        //Dictionary of StreetCrossings to their stringed position
        public static Dictionary<string, StreetCrossing> MappedStreetCrossings = new Dictionary<string, StreetCrossing>();

        //Singleton
        private static StreetCrossingControl instance = null;

        private StreetCrossingControl()
        {
            //Load the pre-calculated StreetCrossings
            List<StreetCrossing> streetCrossings = FileControl.LoadDataFromJsonFile<List<StreetCrossing>>("resources/pedSyncer/StreetCrossings.json");
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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace pedSyncer.control
{
    class PedModelGenerator
    {
        /**
         * 
         * SINGLETON
         * 
         */
        private static PedModelGenerator instance = null;

        public static PedModelGenerator getInstance()
        {
            if (instance == null) instance = new PedModelGenerator();
            return instance;
        }

        /**
         * 
         * OBJECT
         * 
         */

        private Dictionary<int, List<int>> ModelsToNavMeshAreas = new Dictionary<int, List<int>>();

        private PedModelGenerator()
        {
            this.ModelsToNavMeshAreas = StreetCrossingControl.LoadDataFromJsonFile<Dictionary<int, List<int>>>("resources/pedSyncer/ModelsToAreas.json");
        }

        public int GetRandomModelByAreaId(int AreaId)
        {
            if (!this.ModelsToNavMeshAreas.ContainsKey(AreaId)) return 0;

            Random RandomKey = new Random();

            return this.ModelsToNavMeshAreas[AreaId][RandomKey.Next(0, this.ModelsToNavMeshAreas[AreaId].Count - 1)];
        }
    }
}

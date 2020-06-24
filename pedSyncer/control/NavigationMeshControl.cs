using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using AltV.Net;
using NavMesh_Graph;

namespace navMesh_Graph_WebAPI
{
    public class NavigationMeshControl
    {
        //Map navMeshes as their IDs as the key
        private Dictionary<int, NavigationMeshPolyFootpath> navMeshes = new Dictionary<int, NavigationMeshPolyFootpath>();

        //Map navMeshes to zones (just like the ped streamer)
        private Dictionary<(int, int), List<NavigationMeshPolyFootpath>> navMeshesMap = new Dictionary<(int, int), List<NavigationMeshPolyFootpath>>();

        //Singleton
        private static NavigationMeshControl instance = null;

        private NavigationMeshControl()
        {
            //Load the pre-calculated navMeshes
            List<NavigationMeshPolyFootpath> navigationMeshesFootpathsList = LoadDataFromDumpFile<List<NavigationMeshPolyFootpath>>("resources/pedSyncer2/newNavigationMeshes.msgpack");
            Alt.Log("NavMeshFootPath Count: " + navigationMeshesFootpathsList.Count);

            //Parse the input and store them to the dictionaries
            foreach (NavigationMeshPolyFootpath navigationMeshesFootpath in navigationMeshesFootpathsList)
            {
                navigationMeshesFootpath.NeighboursObjects = new List<NavigationMeshPolyFootpath>();

                //But this navMesh to the navMesh-dictionary
                navMeshes.Add(navigationMeshesFootpath.Id, navigationMeshesFootpath);

                //Calculate the zone of this navMesh and store this navMesh to the calculated zone
                int cellX = (int)Math.Ceiling(navigationMeshesFootpath.Position.X / 100.0), cellY = (int)Math.Ceiling(navigationMeshesFootpath.Position.Y / 100.0);
                if (!navMeshesMap.ContainsKey((cellX, cellY))) navMeshesMap.Add((cellX, cellY), new List<NavigationMeshPolyFootpath>());
                navMeshesMap[(cellX, cellY)].Add(navigationMeshesFootpath);
            }

            //store the neighbours of a navMesh as an object
            foreach (NavigationMeshPolyFootpath navigationMeshesFootpath in navigationMeshesFootpathsList)
            {
                foreach (int neighbourId in navigationMeshesFootpath.Neighbours)
                {
                    navigationMeshesFootpath.NeighboursObjects.Add(navMeshes[neighbourId]);
                }
            }
        }

        //Function to get the navMesh given by the permission (check if position is in a polygon)
        public NavigationMeshPolyFootpath getMeshByPosition(WorldVector3 Position)
        {
            //Check if position is in one of the zones
            int cellX = (int)Math.Ceiling(Position.X / 100.0), cellY = (int)Math.Ceiling(Position.Y / 100.0);
            if (
                !navMeshesMap.ContainsKey((cellX, cellY)) &&
                !navMeshesMap.ContainsKey((cellX, cellY - 1)) &&
                !navMeshesMap.ContainsKey((cellX, cellY + 1)) &&
                !navMeshesMap.ContainsKey((cellX - 1, cellY)) &&
                !navMeshesMap.ContainsKey((cellX - 1, cellY - 1)) &&
                !navMeshesMap.ContainsKey((cellX - 1, cellY + 1)) &&
                !navMeshesMap.ContainsKey((cellX + 1, cellY)) &&
                !navMeshesMap.ContainsKey((cellX + 1, cellY - 1)) &&
                !navMeshesMap.ContainsKey((cellX + 1, cellY + 1))
                ) return null;

            //Iterate over all navMeshes in the zones related to the position - return the first match
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    //Check if zone exists
                    if (navMeshesMap.ContainsKey((cellX + i, cellY + j)))
                    {
                        foreach (NavigationMeshPolyFootpath navMesh in navMeshesMap[(cellX + i, cellY + j)])
                        {
                            //Is position in this polygon of the navMesh? If yes, return it
                            if (isPointInPolygon(navMesh.Vertices, Position)) return navMesh;
                        }
                    }
                }
            }
            return null;
        }

        //Calculate a random path starting with the given navMesh
        public List<NavigationMeshPolyFootpath> getRandomPathByMesh(NavigationMeshPolyFootpath startNavMesh)
        {
            if (startNavMesh == null) return new List<NavigationMeshPolyFootpath>();

            //Prepare the path
            List<NavigationMeshPolyFootpath> path = new List<NavigationMeshPolyFootpath>();

            //Add start navMesh to the path
            path.Add(startNavMesh);

            //If the given navMesh doesn't have a neighbour: Stop
            if (startNavMesh.NeighboursObjects.Count == 0) return path;

            //Select a random neighbour of this navMesh to start
            NavigationMeshPolyFootpath randomNavMesh = getRandomNeighbour(startNavMesh);

            //If the random navMesh is null (no neighbours could be selected): Stop
            if (randomNavMesh == null) return path;

            //Add random navMesh to the path
            path.Add(randomNavMesh);

            //Repeat till the path includes 50 stations
            for (int i = path.Count; i < 50; i++)
            {
                //Expand the previous navMesh, select the neighbour by the given directional angle
                NavigationMeshPolyFootpath pathMesh = expandPathByMesh(path[path.Count - 1],
                    WorldVector3.directionalAngle(path[path.Count - 2].Position, path[path.Count - 1].Position), path);

                //If this new station is noll (no neighbours could be selected): Stop
                if (pathMesh == null) return path;

                //Add this navMesh to the path
                path.Add(pathMesh);
            }

            return path;
        }

        //Calculate a random path starting with the given navMesh and a directional angle
        public List<NavigationMeshPolyFootpath> getRandomPathByMeshAndGon(NavigationMeshPolyFootpath startNavMesh, double gon)
        {
            //Prepare the path
            List<NavigationMeshPolyFootpath> path = new List<NavigationMeshPolyFootpath>();

            //Add start navMesh to the path
            path.Add(startNavMesh);

            //Expand the starting navMesh, select the neighbour by the given directional angle
            NavigationMeshPolyFootpath pathMesh = expandPathByMesh(startNavMesh, gon);

            //If this new station is noll (no neighbours could be selected): Stop
            if (pathMesh == null) return path;

            //Add this navMesh to the path
            path.Add(pathMesh);

            for (int i = path.Count; i < 50; i++)
            {
                //Expand the previous navMesh, select the neighbour by the given directional angle
                pathMesh = expandPathByMesh(path[path.Count - 1],
                    WorldVector3.directionalAngle(path[path.Count - 2].Position, path[path.Count - 1].Position), path);

                //If this new station is noll (no neighbours could be selected): Stop
                if (pathMesh == null) return path;

                //Add this navMesh to the path
                path.Add(pathMesh);
            }

            return path;
        }

        //Select a random neighbour by a given navMesh
        public NavigationMeshPolyFootpath getRandomNeighbour(NavigationMeshPolyFootpath startNavMesh)
        {
            //Calculate the random key of the random neighbour
            int randomKey = new Random().Next(0, startNavMesh.NeighboursObjects.Count - 1);

            return startNavMesh.NeighboursObjects[randomKey];
        }

        /*
         * Expand the given navMesh and give that neighbour navMesh back which has the minimum difference to the given gon
         *
         * Optional: Exclude the navMeshes which are already included in the path calculated till now
         */
        public NavigationMeshPolyFootpath expandPathByMesh(NavigationMeshPolyFootpath startNavMesh, double preGon, List<NavigationMeshPolyFootpath> pathTillHere = null)
        {
            //Prepare the minimum difference search
            double minDifference = 500;
            NavigationMeshPolyFootpath minNavMesh = null;

            //If this neighbour hasn't any neighbours: Stop
            if (startNavMesh.NeighboursObjects.Count == 0) return minNavMesh;

            //Iterate over all neighbours of the given startNavMesh
            foreach (NavigationMeshPolyFootpath neighbourMesh in startNavMesh.NeighboursObjects)
            {
                /*
                 * If the difference of the given gon and the gon calculated by the startNavMesh to the current viewed member
                 * is smaller than the current minimum and the neighbour is not already visited: store it.
                 */
                if (Math.Abs(preGon - WorldVector3.directionalAngle(startNavMesh.Position, neighbourMesh.Position)) <
                    minDifference && ((pathTillHere != null && !pathTillHere.Contains(neighbourMesh)) || pathTillHere == null))
                {
                    minDifference =
                        Math.Abs(preGon - WorldVector3.directionalAngle(startNavMesh.Position, neighbourMesh.Position));
                    minNavMesh = neighbourMesh;
                }
            }
            return minNavMesh;
        }

        /*
         * Select random navMeshes at a spawn distributed over the whole map
         *
         * The idea is to view the list of all possible navMesh-footpaths and divide it in sections with the length of
         * randomValueMax. Select in this section a random spawn. Guarantees a distribution of the peds over the whole
         * map.
         */
        public List<NavigationMeshPolyFootpath> getRandomSpawnMeshes()
        {
            //Prepare the spawn list
            List<NavigationMeshPolyFootpath> spawns = new List<NavigationMeshPolyFootpath>();

            Random randomizer = new Random();

            //60: Determines the count of random spawns - there are 98,000 footpath navMeshes, divide by 60: 1633 spawns for peds
            //Todo: make this value as a parameter beginning from the endpoint
            int randomValueMax = 60, randomValue;
            int counter = 0;

            while (randomValueMax * counter < navMeshes.Count)
            {
                //Select a random navMesh in this section
                randomValue = randomizer.Next(0, randomValueMax);
                spawns.Add(navMeshes[counter * randomValue]);
                counter++;
            }

            return spawns;
        }

        /**
         * Singleton-Methods
         */
        public static NavigationMeshControl getInstance()
        {
            if (instance == null) instance = new NavigationMeshControl();
            return instance;
        }

        /**
         * Support-Methods
         */
        static TDumpType LoadDataFromDumpFile<TDumpType>(string dumpFileName)
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

        private static bool isPointInPolygon(List<WorldVector3> polygon, WorldVector3 testPoint)
        {
            bool result = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}

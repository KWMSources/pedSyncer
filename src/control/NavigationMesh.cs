using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using AltV.Net;
using System.Numerics;
using PedSyncer.Utils;
using PedSyncer.Model;

namespace PedSyncer.Control
{
    public class NavigationMesh
    {
        //Map navMeshes as their IDs as the key
        private Dictionary<int, NavigationMeshPolyFootpath> navMeshes = new Dictionary<int, NavigationMeshPolyFootpath>();

        //Map navMeshes to zones (just like the ped streamer)
        private Dictionary<(int, int), List<NavigationMeshPolyFootpath>> navMeshesMap = new Dictionary<(int, int), List<NavigationMeshPolyFootpath>>();

        //Singleton
        private static NavigationMesh? instance = null;

        private NavigationMesh()
        {
            //Load the pre-calculated navMeshes
            List<NavigationMeshPolyFootpath> navigationMeshesFootpathsList = FileControl.LoadDataFromDumpFile<List<NavigationMeshPolyFootpath>>("resources/pedSyncer/server/newNavigationMeshes.msgpack");
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

        //Function to get the navMesh given by the current position (check if position is in a polygon)
        public NavigationMeshPolyFootpath? getMeshByPosition(Vector3 Position)
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

        //Function to get the nearest navMesh given by the current position (check if position is in a polygon)
        public NavigationMeshPolyFootpath? getNearestMeshByPosition(Vector3 Position)
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
            NavigationMeshPolyFootpath? minMesh = null;
            float minValue = 999999;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    //Check if zone exists
                    if (navMeshesMap.ContainsKey((cellX + i, cellY + j)))
                    {
                        foreach (NavigationMeshPolyFootpath navMesh in navMeshesMap[(cellX + i, cellY + j)])
                        {
                            float distance = Vector3.Distance(Position, navMesh.Position);
                            if (distance < minValue)
                            {
                                minValue = distance;
                                minMesh = navMesh;
                            }
                        }
                    }
                }
            }
            return minMesh;
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
        public static NavigationMesh getInstance()
        {
            if (instance == null) instance = new NavigationMesh();
            return instance;
        }

        /**
         * Support-Methods
         */
        //Method to determine if a given position is in a polygon
        private static bool isPointInPolygon(List<Vector3> polygon, Vector3 testPoint)
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

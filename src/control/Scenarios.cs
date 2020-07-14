using AltV.Net;
using PedSyncer.Model;
using PedSyncer.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PedSyncer.Control
{
    class Scenarios
    {
        //Singleton
        private static Scenarios? instance = null;

        public static Scenarios getInstance()
        {
            if (instance == null) instance = new Scenarios();
            return instance;
        }

        //Objekt
        Dictionary<string, List<string>> PedModelGroups;
        List<ScenarioPoint> ScenarioPoints = new List<ScenarioPoint>();
        List<string> AllowedScenarios;
        List<int> NonePedGroupModels = new List<int>();

        //Map of the ScenarioPoints, Mapped by 10x10
        Dictionary<(int, int), List<ScenarioPoint>> ScenarioMap = new Dictionary<(int, int), List<ScenarioPoint>>();

        private Scenarios()
        {
            //Load required JSONs
            PedModelGroups = FileControl.LoadDataFromJsonFile<Dictionary<string, List<string>>>("resources/pedSyncer/server/PedModelGroup.json");
            AllowedScenarios = FileControl.LoadDataFromJsonFile<List<string>>("resources/pedSyncer/server/Scenarios.json");

            //Load Scenario Points JSON, parse only the "allowed scenarios" and map them
            List<ScenarioPoint> ScenarioPointsTemp = FileControl.LoadDataFromJsonFile<List<ScenarioPoint>>("resources/pedSyncer/server/ScenarioPoints.json");
            foreach (ScenarioPoint scenarioPoint in ScenarioPointsTemp)
            {
                if (AllowedScenarios.Contains(scenarioPoint.IType))
                {
                    ScenarioPoints.Add(scenarioPoint);

                    int cellX = (int)Math.Ceiling(scenarioPoint.Position.X / 10), 
                        cellY = (int)Math.Ceiling(scenarioPoint.Position.Y / 10);

                    if (!ScenarioMap.ContainsKey((cellX, cellY))) ScenarioMap.Add((cellX, cellY), new List<ScenarioPoint>());

                    ScenarioMap[(cellX, cellY)].Add(scenarioPoint);
                }
            }

            /*
             * Determine the near scenario points to each scenario point
             * 
             * If a scenario point is close (but not too close) to an other scenario point the probability is high
             * that these scenario points are related to each other (like an discussion between two peds)
             */
            foreach (ScenarioPoint scenarioPoint2 in ScenarioPoints)
            {
                int cellX = (int)Math.Ceiling(scenarioPoint2.Position.X / 10),
                    cellY = (int)Math.Ceiling(scenarioPoint2.Position.Y / 10);

                scenarioPoint2.NearScenarioPoint = new List<ScenarioPoint>();

                if (
                    !ScenarioMap.ContainsKey((cellX, cellY)) &&
                    !ScenarioMap.ContainsKey((cellX, cellY - 1)) &&
                    !ScenarioMap.ContainsKey((cellX, cellY + 1)) &&
                    !ScenarioMap.ContainsKey((cellX - 1, cellY)) &&
                    !ScenarioMap.ContainsKey((cellX - 1, cellY - 1)) &&
                    !ScenarioMap.ContainsKey((cellX - 1, cellY + 1)) &&
                    !ScenarioMap.ContainsKey((cellX + 1, cellY)) &&
                    !ScenarioMap.ContainsKey((cellX + 1, cellY - 1)) &&
                    !ScenarioMap.ContainsKey((cellX + 1, cellY + 1))
                ) continue;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        //Check if zone exists
                        if (ScenarioMap.ContainsKey((cellX + i, cellY + j)))
                        {
                            foreach (ScenarioPoint scenarioPoint3 in ScenarioMap[(cellX + i, cellY + j)])
                            {
                                double distance = Vector3Utils.GetDistanceBetweenPos(
                                        new Vector3(scenarioPoint2.Position.X, scenarioPoint2.Position.Y, scenarioPoint2.Position.Z),
                                        new Vector3(scenarioPoint3.Position.X, scenarioPoint3.Position.Y, scenarioPoint3.Position.Z)
                                    );

                                if (distance < 3 && distance > 1)
                                {
                                    scenarioPoint2.NearScenarioPoint.Add(scenarioPoint3);
                                }
                            }
                        }
                    }
                }
            }

            //Save Ped Models for the none-pedgroup
            Dictionary<int, List<int>> ModelsToAreas = FileControl.LoadDataFromJsonFile<Dictionary<int, List<int>>>("resources/pedSyncer/server/ModelsToAreas.json");
            foreach (List<int> ModelsToArea in ModelsToAreas.Values)
            {
                foreach (int Model in ModelsToArea)
                {
                    if (!NonePedGroupModels.Contains(Model)) NonePedGroupModels.Add(Model);
                }
            }
        }

        /**
         * Method to get a list of randomly selected ScenarioPoints
         */
        public List<ScenarioPoint> GetRandomScenarioSpots()
        {
            //Currently spawns 3000 scenario peds
            int ToSelect = 3000;

            Random Randomizer = new Random();
            List<ScenarioPoint> ScenarioSpawns = new List<ScenarioPoint>();

            //Select another scenario point if the ToSelect variable is not below zero
            while (ToSelect > 0)
            {
                ScenarioPoint ScenarioPoint = ScenarioPoints[Randomizer.Next(0, ScenarioPoints.Count - 1)];

                if (!ScenarioSpawns.Contains(ScenarioPoint))
                {
                    ScenarioSpawns.Add(ScenarioPoint);
                    ToSelect--;

                    //If this ScenarioPoint has other ScenarioPoints near to it: Add them too
                    if (ScenarioPoint.NearScenarioPoint.Count > 0)
                    {
                        foreach (ScenarioPoint ScenarioPoint2 in ScenarioPoint.NearScenarioPoint)
                        {
                            ScenarioSpawns.Add(ScenarioPoint2);
                            ToSelect--;
                        }
                    }
                }
            }

            return ScenarioSpawns;
        }

        //Get a random model by a given scenario point
        public string GetRandomModelByScenarioPoint(ScenarioPoint ScenarioPoint)
        {
            Random Randomizer = new Random();

            //If the ModelType is none or there are no peds in the ModelType: Use the standard ped models
            if (
                ScenarioPoint.ModelType == "none" || 
                !PedModelGroups.ContainsKey(ScenarioPoint.ModelType.ToUpper())
            ) return Ped.ParseModelHash(NonePedGroupModels[Randomizer.Next(0, NonePedGroupModels.Count - 1)]);

            return PedModelGroups[ScenarioPoint.ModelType.ToUpper()][Randomizer.Next(0, PedModelGroups[ScenarioPoint.ModelType.ToUpper()].Count - 1)];
        }
    }
}

using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using PedSyncer.Model;
using PedSyncer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedSyncer.Control
{
    internal class PedVehicles
    {
        private static PedVehicles? Instance = null;

        //Start the vehicles controller
        private PedVehicles()
        {
            StreetNodeAreas = FileControl.LoadDataFromJsonFile<List<StreetNodeArea>>("resources/pedSyncer/server/StreetNodes.json");
            foreach (StreetNodeArea streetNodeArea in StreetNodeAreas)
            {
                foreach (StreetNode streetNode in streetNodeArea.Nodes)
                {
                    if (!streetNode.IsPedCrossway && !streetNode.IsOnWater && !streetNode.IsGravelRoad && streetNode.StreetName != "0") StreetNodes.Add(streetNode);
                }
            }

            CarModels = FileControl.LoadDataFromJsonFile<List<string>>("resources/pedSyncer/server/data/CarModels.json");
            ColorlessCars = FileControl.LoadDataFromJsonFile<List<string>>("resources/pedSyncer/server/data/ColorlessCars.json");
            CarColorsNum = FileControl.LoadDataFromJsonFile<List<int>>("resources/pedSyncer/server/data/CarColorsNum.json");
        }

        //Singleton
        public static PedVehicles GetInstance()
        {
            if (Instance == null) Instance = new PedVehicles();
            return Instance;
        }

        private List<StreetNodeArea> StreetNodeAreas;
        private List<StreetNode> StreetNodes = new List<StreetNode>();
        private List<StreetNode> SpawnStreetNodes = new List<StreetNode>();

        private List<string> CarModels;
        private List<string> ColorlessCars;
        private List<int> CarColorsNum;

        private string[] NumberPlateChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private string GenerateNumberPlate()
        {
            Random randomizer = new Random();

            string numberplate = "";

            for (int i = 0; i < 8; i++) numberplate += NumberPlateChars[randomizer.Next(0, NumberPlateChars.Length - 1)];

            return numberplate;
        }

        public void SpawnRandomCitizenVehicles(int VehicleCount)
        {
            float RandomMaxFloat = StreetNodes.ToArray().Length / VehicleCount;
            int RandomMax = Convert.ToInt32(RandomMaxFloat);

            Random Randomizer = new Random();

            for (int i = 0; (i+1) * RandomMax < StreetNodes.Count; i++)
            {
                SpawnStreetNodes.Add(StreetNodes[(i * RandomMax) + Randomizer.Next(0, RandomMax)]);
            }

            foreach (StreetNode StreetNode in SpawnStreetNodes)
            {
                StreetNodeConnected? ForwardNode = null;
                foreach (StreetNodeConnected streetNodeConnected in StreetNode.ConnectedNodes)
                {
                    if (streetNodeConnected.LaneCountForward > 0)
                    {
                        ForwardNode = streetNodeConnected;
                        break;
                    }
                }

                if (ForwardNode == null) return;

                string model = CarModels[Randomizer.Next(0, CarModels.Count - 1)];

                //Create the vehicle
                IVehicle vehicle = Alt.CreateVehicle(
                    model,
                    new Position(StreetNode.Position.X, StreetNode.Position.Y, StreetNode.Position.Z + 2),
                    new Rotation(0, 0, 0 -
                        (float)Math.Atan2(
                         Convert.ToDouble(ForwardNode.Node.Position.X - StreetNode.Position.X),
                         Convert.ToDouble(ForwardNode.Node.Position.Y - StreetNode.Position.Y)
                        )
                    )
                );

                Ped Ped = new Ped(StreetNode.Position.X, StreetNode.Position.Y, StreetNode.Position.Z + 2);
                Ped.SetRandomModel();
                Ped.Vehicle = vehicle;
                Ped.Seat = -1;

                vehicle.SetSyncedMetaData("ped", Ped.Id);

                //Set the vehicle color
                if (!ColorlessCars.Contains(model))
                {
                    if (CarColorsNum.Count != 0)
                    {
                        byte color = (byte)CarColorsNum[Randomizer.Next(0, CarColorsNum.Count - 1)];
                        vehicle.PrimaryColor = color;
                        vehicle.SecondaryColor = color;
                    }
                }

                vehicle.NumberplateText = GenerateNumberPlate();
            }
        }
    }
}

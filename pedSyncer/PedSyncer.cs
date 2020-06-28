using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.ServerEvent;
using AltV.Net.EntitySync.SpatialPartitions;
using navMesh_Graph_WebAPI;
using pedSyncer.control;
//using pedSyncer.Task;

namespace PedSyncer
{
    internal class PedSyncer : Resource
    {            
        //Define here if u want to activate the DebugMode Clientside
        public static bool DebugModeClientSide = false;
        //Define here if u want to activate the DebugMode ServerSide
        public static bool DebugModeServerSide = false;

        private void InitEntitySync()
        {
            AltEntitySync.Init(
                4,
                100,
                (threadCount, repository) => new ServerEventNetworkLayer(threadCount, repository),
                (entity, threadCount) => (entity.Id % threadCount),
                (entityId, entityType, threadCount) => (entityId % threadCount),
                (threadId) => new LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 256),
                new IdProvider()
            );

            Console.WriteLine("[INFO] GameEntityResource InitEntitySync startet");
        }

        async public override void OnStart()
        {
            this.InitEntitySync();

            Alt.OnClient<IPlayer, Dictionary<string, string>>("pedSyncer:client:firstSpawn", Controller.OnFirstSpawn);
            Alt.OnClient<IPlayer, object[]>("pedSyncer:client:positions", Controller.OnPositionUpdate);

            Alt.OnPlayerConnect += Controller.OnPlayerConnect;

            AltEntitySync.OnEntityCreate += Controller.OnEntityCreate;
            AltEntitySync.OnEntityRemove += Controller.OnEntityRemove;

            Console.WriteLine("Started");

            NavigationMeshControl.getInstance();
            StreetCrossingControl.getInstance();
            PedMovementControl.GetInstance();

            Ped.CreateCitizenPeds();
        }
        public override void OnTick()
        {
            //TaskRunning.OnTick();
        }
        public override void OnStop()
        {
            Console.WriteLine("Stopped");
        }
    }
}

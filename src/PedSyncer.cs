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
using pedSyncer;
using PedSyncer.Control;
using PedSyncer.Model;
//using pedSyncer.Task;

namespace PedSyncer
{
    internal class PedSyncer : Resource
    {            
        //Define here if u want to activate the DebugMode Clientside
        public static bool DebugModeClientSide = true;
        //Define here if u want to activate the DebugMode ServerSide
        public static bool DebugModeServerSide = true;

        private void InitEntitySync()
        {
            //Prepare the EntitySync - Ped Limit is 256
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

            /**
             * Prepare the Events and route them to the controllers
             */
            Alt.OnClient<IPlayer, Dictionary<string, string>>("pedSyncer:client:firstSpawn", Events.OnFirstSpawn);
            Alt.OnClient<IPlayer, object[]>("pedSyncer:client:positions", Events.OnPositionUpdate);

            Alt.OnPlayerConnect += Events.OnPlayerConnect;

            AltEntitySync.OnEntityCreate += Events.OnEntityCreate;
            AltEntitySync.OnEntityRemove += Events.OnEntityRemove;

            Console.WriteLine("Started");

            /**
             * Load all files (navMeshes & StreetCrossing)
             */
            NavigationMesh.getInstance();
            StreetCrossingControl.getInstance();

            //Start serverside ped movement calculation
            PedMovement.GetInstance();

            //Create citizen peds who wanders - delete this line if you don't wanna have citizens
            Ped.CreateCitizenPeds();

            //NodeJS Wrapper
            PedSyncerWrapper.RegisterWrapperFunctions();
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

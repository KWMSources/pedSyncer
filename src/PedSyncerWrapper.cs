using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AltV.Net;
using PedSyncer;
using PedSyncer.Model;

namespace pedSyncer
{
    class PedSyncerWrapper
    {
        public static void RegisterWrapperFunctions()
        {
            Console.WriteLine("[INFO] PedSyncer Wrapper Functions created.");
            Alt.Export("setPedData", new Action<ulong, string, object>(PedSyncerWrapper.SetPedData));
            Alt.Export("getPedData", new Func<ulong, string, object>(PedSyncerWrapper.GetPedData));
            Alt.Export("createPed", new Func<float, float, float, string, ulong>(PedSyncerWrapper.CreatePed));
            Alt.Export("destroyPed", new Action<ulong>(PedSyncerWrapper.DestroyPed));
            Alt.Export("startWandering", new Action<ulong>(PedSyncerWrapper.StartWandering));
            Alt.Export("continueWandering", new Action<ulong>(PedSyncerWrapper.ContinueWandering));
            Alt.Export("getAllPedIds", new Func<ulong[]>(PedSyncerWrapper.GetAllPedIds));
            Alt.Export("isPedIdExisting", new Func<ulong, bool>(PedSyncerWrapper.IsPedIdExisting));
            Alt.Export("getNear", new Func<Vector3, float, ulong[]>(PedSyncerWrapper.GetNear));
            Alt.Export("createCitizenPeds", new Action(PedSyncerWrapper.CreateCitizenPeds));
        }

        public static void SetPedData(ulong Id, string Key, object Value)
        {
            Ped? Ped = Ped.GetByID(Id);
            if (Ped == null || Ped.GetType().GetProperty(Key) == null) return;

            Ped.GetType().GetProperty(Key).SetValue(Ped, Value);
        }

        public static object? GetPedData(ulong Id, string Key)
        {
            Ped? Ped = Ped.GetByID(Id);
            if (Ped == null || Ped.GetType().GetProperty(Key) == null) return null;

            return Ped.GetType().GetProperty(Key).GetValue(Ped);
        }

        /*public static void SetPedMetaData(ulong Id, string Key, object Value)
        {
            Ped Ped = Ped.GetByID(Id);
            if (Ped == null || Ped.GetType().GetProperty(Key) == null) return;

            Ped.SetMetaData(Key, value);
        }

        public static object GetPedMetaData(ulong Id, string Key)
        {

        }

        public static object SetPedSyncedMetaData(ulong Id, string Key, object Value)
        {

        }

        public static object GetPedSyncedMetaData(ulong Id, string Key)
        {

        }

        public static object SetPedStreamedSyncedMetaData(ulong Id, string Key, object Value)
        {

        }

        public static object GetPedStreamedSyncedMetaData(ulong Id, string Key)
        {

        }*/

        public static ulong CreatePed(float x, float y, float z, string? model = null)
        {
            Ped ped = new Ped(x, y, z, model);
            
            return ped.Id;
        }

        public static void DestroyPed(ulong Id)
        {
            Ped? Ped = Ped.GetByID(Id);
            if (Ped == null) return;

            Ped.Destroy();
        }

        public static void StartWandering(ulong Id)
        {
            Ped? Ped = Ped.GetByID(Id);
            if (Ped == null) return;

            Ped.Wandering = true;
        }

        public static void ContinueWandering(ulong Id)
        {
            Ped? Ped = Ped.GetByID(Id);
            if (Ped == null) return;

            Ped.ContinueWandering();
        }

        public static ulong[] GetAllPedIds()
        {
            List<ulong> Ids = new List<ulong>();

            foreach(Ped ped in Ped.All)
            {
                Ids.Add(ped.Id);
            }

            return Ids.ToArray();
        }

        public static bool IsPedIdExisting(ulong Id)
        {
            Ped? Ped = Ped.GetByID(Id);

            return Ped != null;
        }

        public static ulong[] GetNear(Vector3 Position, float Distance)
        {
            List<ulong> Ids = new List<ulong>();

            foreach (Ped ped in Ped.GetNear(Position, Distance))
            {
                Ids.Add(ped.Id);
            }

            return Ids.ToArray();
        }

        public static void CreateCitizenPeds()
        {
            Console.WriteLine("[INFO] PedSyncer Wrapper Create Citizens.");
            Ped.CreateCitizenPeds();
        }
    }
}

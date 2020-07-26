using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.ServerEvent;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using PedSyncer.Model;

namespace PedSyncer.Control
{
    public class Events
    {
        /**
         * Event which is fired on the time the first netOwner of a ped created the ped and decided
         * the 'look' of the ped.
         */
        public static void OnFirstSpawn(IPlayer Player, Dictionary<string, string> PedClient)
        {
            if (!PedClient.TryGetValue("id", out string? idStr)) return;

            ulong id = (ulong)int.Parse(idStr);

            Ped? ped = Ped.GetByID(id);

            if (ped == null) return;

            //Alt.EmitAllClients("pedSyncer:server:update", ped);
        }

        /**
         * Event which is fired on the sending of the current positions and other states of 
         * the peds by their netOwner. This event will be fired very, very often.
         */
        public static void OnPositionUpdate(IPlayer Player, object[] PedClientArray)
        {
            if (PedClientArray.Length == 0) return;

            //Process all sended peds
            foreach (Dictionary<string, object> PedClient in PedClientArray)
            {
                if (!PedClient.TryGetValue("id", out object? idStr)) continue;

                ulong Id = (ulong)IntegerType.FromObject(idStr);

                //Get the ped which should be updated
                Ped? ped = Ped.GetByID(Id);

                if (ped == null) return;

                //Update position
                if (PedClient.TryGetValue("pos", out object? posObj))
                {
                    ped.Position = JsonConvert.DeserializeObject<Vector3>((string)posObj);
                }

                //Update heading
                if (PedClient.TryGetValue("heading", out object? headingObj)) ped.Heading = Convert.ToDouble(headingObj);

                //Update nearFinalPosition state
                if (PedClient.TryGetValue("nearFinalPosition", out object? nearFinalObject))
                {
                    bool nearFinal = (bool)nearFinalObject;
                    if (nearFinal && !ped.NearFinalPosition)
                    {
                        ped.NearFinalPosition = true;
                        ped.ContinueWandering();
                    }
                }

                //Update armour
                if (PedClient.TryGetValue("armour", out object? armourObj)) ped.Armour = Convert.ToInt32(armourObj);

                //Update health
                if (PedClient.TryGetValue("health", out object? healthObj)) ped.Health = Convert.ToInt32(healthObj);

                //Update dead
                if (PedClient.TryGetValue("dead", out object? deadObj)) ped.Dead = Convert.ToBoolean(deadObj);

                //Set Ped flags
                /*if (PedClient.TryGetValue("flags", out object[] flagsObj))
                {
                    List<bool> flagsBool = new List<bool>();
                    foreach(object flag in flagsObj)
                    {
                        flagsBool.Add(Convert.ToBoolean(flag));
                    }
                    ped.SetFlags(flagsBool.ToArray());
                }*/
            }
        }

        /**
         * On connection of a player:
         * 1. send all peds to the player
         * 2. send the debug stats to the player
         */
        public static void OnPlayerConnect(IPlayer player, string reason)
        {
            Parallel.ForEach(Ped.All, ped =>
            {
                player.EmitLocked("pedSyncer:server:create", ped);
            });

            if (PedSyncer.DebugModeClientSide)
            {
                player.EmitLocked("pedSyncer:debugmode", true);
            }
        }

        /**
         * Event which fires if a ped was streamed out of a player
         * 
         * Determine the new netOwner or start the serverside ped movement calculcation
         */
        public static void OnEntityRemove(IClient client, AltV.Net.EntitySync.IEntity entity)
        {
            if (entity.Type != Ped.PED_TYPE) return;

            Ped? ped = Ped.GetByID(entity.Id);

            if (ped == null) return;

            HashSet<IClient> PedStreamedToClients = ped.GetClients();

            //No other players got the ped streamed in: Start serverside ped movement calculation
            if (PedStreamedToClients.Count == 1)
            {
                ped.NetOwner = null;
                PedMovement.GetInstance().AddPedMovementCalculcation(ped);
            }
            //An other player got the ped streamed in, set him as the new netOwner
            else
            {
                IClient[] pNewClients = PedStreamedToClients.ToArray<IClient>();

                //Determine the first valid player
                foreach (IClient pNewClient in pNewClients)
                {
                    IPlayer pNewPlayer = ((PlayerClient)pNewClient).GetPlayer();

                    if (pNewPlayer.Exists && (ped.NetOwner == null || pNewPlayer.Id != ((PlayerClient)ped.NetOwner).GetPlayer().Id))
                    {
                        ped.NetOwner = pNewClient;
                        return;
                    }
                }

                //No valid player found, start serverside ped movement calculation
                ped.NetOwner = null;
                PedMovement.GetInstance().AddPedMovementCalculcation(ped);
            }
        }

        public static void OnTaskUpdate(IPlayer Player, ulong PedId, string TaskString, string[] TaskParams)
        {
            Ped? Ped = Ped.GetByID(PedId);

            if (Ped == null) return;

            if (Ped.NetOwner != null && ((PlayerClient)Ped.NetOwner).GetPlayer().Id != Player.Id) return;

            bool ParamCheck = false;
            if (TaskParams.Length != Ped.TaskParams.Count) ParamCheck = true;
            else
            {
                for (int i = 0; i < TaskParams.Length; i++)
                {
                    if (TaskParams[i] != Ped.TaskParams[i])
                    {
                        ParamCheck = true;
                        break;
                    }
                }
            }


            if (Ped.Task != TaskString || ParamCheck)
            {
                Ped.TaskParams = TaskParams.ToList<string>();
                Ped.Task = TaskString;
            }
        }
    }
}
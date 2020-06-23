using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.ServerEvent;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PedSyncer
{
	public class Controller
	{
		public Controller()
		{
		}

		public static void OnFirstSpawn(IPlayer Player, Dictionary<string,string> PedClient)
        {
			if (!PedClient.TryGetValue("id", out string idStr)) return;
			foreach(string key in PedClient.Keys)
            {
				Console.WriteLine("Test First Spawn " + key + " " + PedClient[key]);
            }

			ulong id = (ulong) IntegerType.FromString(idStr);

			Ped ped = Ped.GetByID(id);

			if (ped == null) return;

			ped.Created = true;

			if (!PedClient.TryGetValue("drawable1", out string drawable1Str)) ped.Drawable1 = 0;
			else ped.Drawable1 = IntegerType.FromString(drawable1Str);

			if (!PedClient.TryGetValue("drawable2", out string drawable2Str)) ped.Drawable2 = 0;
			else ped.Drawable2 = IntegerType.FromString(drawable2Str);

			if (!PedClient.TryGetValue("drawable3", out string drawable3Str)) ped.Drawable3 = 0;
			else ped.Drawable3 = IntegerType.FromString(drawable3Str);

			if (!PedClient.TryGetValue("drawable4", out string drawable4Str)) ped.Drawable4 = 0;
			else ped.Drawable4 = IntegerType.FromString(drawable4Str);

			if (!PedClient.TryGetValue("drawable5", out string drawable5Str)) ped.Drawable5 = 0;
			else ped.Drawable5 = IntegerType.FromString(drawable5Str);

			if (!PedClient.TryGetValue("drawable6", out string drawable6Str)) ped.Drawable6 = 0;
			else ped.Drawable6 = IntegerType.FromString(drawable6Str);

			if (!PedClient.TryGetValue("drawable7", out string drawable7Str)) ped.Drawable7 = 0;
			else ped.Drawable7 = IntegerType.FromString(drawable7Str);

			if (!PedClient.TryGetValue("drawable8", out string drawable8Str)) ped.Drawable8 = 0;
			else ped.Drawable8 = IntegerType.FromString(drawable8Str);

			if (!PedClient.TryGetValue("drawable9", out string drawable9Str)) ped.Drawable9 = 0;
			else ped.Drawable9 = IntegerType.FromString(drawable9Str);

			if (!PedClient.TryGetValue("drawable10", out string drawable10Str)) ped.Drawable10 = 0;
			else ped.Drawable10 = IntegerType.FromString(drawable10Str);

			if (!PedClient.TryGetValue("drawable11", out string drawable11Str)) ped.Drawable11 = 0;
			else ped.Drawable11 = IntegerType.FromString(drawable11Str);


			if (!PedClient.TryGetValue("texture1", out string texture1Str)) ped.Texture1 = 0;
			else ped.Texture1 = IntegerType.FromString(texture1Str);

			if (!PedClient.TryGetValue("texture2", out string texture2Str)) ped.Texture2 = 0;
			else ped.Texture2 = IntegerType.FromString(texture2Str);

			if (!PedClient.TryGetValue("texture3", out string texture3Str)) ped.Texture3 = 0;
			else ped.Texture3 = IntegerType.FromString(texture3Str);

			if (!PedClient.TryGetValue("texture4", out string texture4Str)) ped.Texture4 = 0;
			else ped.Texture4 = IntegerType.FromString(texture4Str);

			if (!PedClient.TryGetValue("texture5", out string texture5Str)) ped.Texture5 = 0;
			else ped.Texture5 = IntegerType.FromString(texture5Str);

			if (!PedClient.TryGetValue("texture6", out string texture6Str)) ped.Texture6 = 0;
			else ped.Texture6 = IntegerType.FromString(texture6Str);

			if (!PedClient.TryGetValue("texture7", out string texture7Str)) ped.Texture7 = 0;
			else ped.Texture7 = IntegerType.FromString(texture7Str);

			if (!PedClient.TryGetValue("texture8", out string texture8Str)) ped.Texture8 = 0;
			else ped.Texture8 = IntegerType.FromString(texture8Str);

			if (!PedClient.TryGetValue("texture9", out string texture9Str)) ped.Texture9 = 0;
			else ped.Texture9 = IntegerType.FromString(texture9Str);

			if (!PedClient.TryGetValue("texture10", out string texture10Str)) ped.Texture10 = 0;
			else ped.Texture10 = IntegerType.FromString(texture10Str);

			if (!PedClient.TryGetValue("texture11", out string texture11Str)) ped.Texture11 = 0;
			else ped.Texture11 = IntegerType.FromString(texture11Str);


			if (!PedClient.TryGetValue("prop0", out string prop0Str)) ped.Prop0 = 0;
			else ped.Prop0 = IntegerType.FromString(prop0Str);

			if (!PedClient.TryGetValue("prop1", out string prop1Str)) ped.Prop1 = 0;
			else ped.Prop1 = IntegerType.FromString(prop1Str);

			if (!PedClient.TryGetValue("prop2", out string prop2Str)) ped.Prop2 = 0;
			else ped.Prop2 = IntegerType.FromString(prop2Str);

			if (!PedClient.TryGetValue("prop6", out string prop6Str)) ped.Prop6 = 0;
			else ped.Prop6 = IntegerType.FromString(prop6Str);

			if (!PedClient.TryGetValue("prop7", out string prop7Str)) ped.Prop7 = 0;
			else ped.Prop7 = IntegerType.FromString(prop7Str);

			if (PedClient.TryGetValue("model", out string model)) ped.Model = model;

			if (PedClient.TryGetValue("gender", out string gender)) ped.Gender = gender;

			Alt.EmitAllClients("pedSyncer:server:update", ped);
		}

		public static void OnPositionUpdate(IPlayer Player, object[] PedClientArray)
        {
			if (PedClientArray.Length == 0) return;
			foreach (Dictionary<string, object> PedClient in PedClientArray)
            {
				if (!PedClient.TryGetValue("id", out object idStr)) continue;

				ulong Id = (ulong)IntegerType.FromObject(idStr);

				Ped ped = Ped.GetByID(Id);

				if (ped == null) return;

				if (PedClient.TryGetValue("pos", out object posObj))
				{
					ped.Position = JsonConvert.DeserializeObject<Vector3>((string)posObj);
				}
				if (PedClient.TryGetValue("heading", out object headingObj)) ped.Heading = Convert.ToDouble(headingObj);
				if (PedClient.TryGetValue("nearFinalPosition", out object nearFinalObject))
                {
					bool nearFinal = (bool)nearFinalObject;
					if (nearFinal && !ped.NearFinalPosition)
                    {
						ped.NearFinalPosition = true;
						ped.ContinueWandering();
					}
                }
			}
		}

		public static void OnPlayerConnect(IPlayer player, string reason)
        {
			foreach (Ped ped in Ped.All) player.Emit("pedSyncer:server:create", ped);
        }

		public static void OnEntityCreate(IClient client, AltV.Net.EntitySync.IEntity entity)
		{
			if (entity.Type != Ped.PED_TYPE) return;

			PlayerClient pClient = (PlayerClient)client;
			IPlayer player = pClient.GetPlayer();

			Ped.GetByID(entity.Id).NetOwner = player;
		}

		public static void OnEntityRemove(IClient client, AltV.Net.EntitySync.IEntity entity)
		{
			if (entity.Type != Ped.PED_TYPE) return;

			Ped ped = Ped.GetByID(entity.Id);

			HashSet<IClient> PedStreamedToClients = ped.GetClients();

			if (PedStreamedToClients.Count == 1)
            {
				ped.NetOwner = null;
				PedMovementControl.GetInstance().AddPedMovementCalculcation(ped);
			} else
            {
				IClient[] pNewClients = PedStreamedToClients.ToArray<IClient>();

				foreach (IClient pNewClient in pNewClients)
				{
					IPlayer pNewPlayer = ((PlayerClient)pNewClient).GetPlayer();

					if (pNewPlayer.Exists && pNewPlayer.Id != ped.NetOwner.Id)
                    {
						ped.NetOwner = pNewPlayer;
						return;
					}
				}

				ped.NetOwner = null;
				PedMovementControl.GetInstance().AddPedMovementCalculcation(ped);
			}
		}
	}
}
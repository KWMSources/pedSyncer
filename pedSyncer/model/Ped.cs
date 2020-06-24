using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using NavMesh_Graph;
using navMesh_Graph_WebAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PedSyncer
{
    public class Ped : AltV.Net.EntitySync.Entity, IWritable
    {

        #region Properties
        public const ulong PED_TYPE = 1654;
        public const int STREAMING_RANGE = 200;
        #endregion

        public static ConcurrentDictionary<ulong, Ped> peds = new ConcurrentDictionary<ulong, Ped>();

        /**
		 * Just one Player is the netOwner of a Ped. This Player has the task to
		 * tell the server the current position of the Ped.
		 *
		 * The first netOwner has also the task to creat the ped on the first time.
		 */

        public IPlayer NetOwner
        {
            get
            {
                if (this.TryGetData<ushort>("netOwner", out ushort value))
                {
                    foreach (IPlayer player in Alt.GetAllPlayers())
                    {
                        if (player.Id == value) return player;
                    }
                }
                return null;
            }
            set
            {
                if (value == null) this.SetData("netOwner", null);
                else this.SetData("netOwner", value.Id);
            }
        }

        /**
		 * Always true currently
		 *
		 * Will give information about the validity
		 * ToDo: When is a ped invalid?
		 */

        public bool Valid
        { get; }

        /**
		 * Tells if the ped was already created on one client
		 *
		 * If created is true this object will contain information about the style
		 * of this ped
		 */

        public bool Created
        {
            get
            {
                if (this.TryGetData<bool>("created", out bool value)) return value;
                return false;
            }
            set
            {
                this.SetData("created", value);
            }
        }

        /**
		 * Current heading of the ped
		 */

        public double Heading
        { get; set; }

        /**
		 * Ped-Style Properties
		 */

        public string Model
        { get; set; }

        public int Drawable1
        { get; set; }

        public int Drawable2
        { get; set; }

        public int Drawable3
        { get; set; }

        public int Drawable4
        { get; set; }

        public int Drawable5
        { get; set; }

        public int Drawable6
        { get; set; }

        public int Drawable7
        { get; set; }

        public int Drawable8
        { get; set; }

        public int Drawable9
        { get; set; }

        public int Drawable10
        { get; set; }

        public int Drawable11
        { get; set; }

        public int Texture1
        { get; set; }

        public int Texture2
        { get; set; }

        public int Texture3
        { get; set; }

        public int Texture4
        { get; set; }

        public int Texture5
        { get; set; }

        public int Texture6
        { get; set; }

        public int Texture7
        { get; set; }

        public int Texture8
        { get; set; }

        public int Texture9
        { get; set; }

        public int Texture10
        { get; set; }

        public int Texture11
        { get; set; }

        public int Prop0
        { get; set; }

        public int Prop1
        { get; set; }

        public int Prop2
        { get; set; }

        public int Prop6
        { get; set; }

        public int Prop7
        { get; set; }

        public string Gender
        { get; set; }

        //Currently inactive - will contain information if the ped is invincible
        public bool Invincible
        { get; set; }

        //Currently inactive - The vehicle the ped sits in
        public IVehicle Vehicle
        { get; set; }

        //Currently inactive - If the ped is in a vehicle, this tells the current seat of the ped
        public int Seat
        { get; set; }

        //Currently inactive - HP-Stats of the ped
        public List<int> Injuries
        { get; set; }

        public bool HasBlood
        { get; }

        public int Armour
        { get; set; }

        public int Health
        { get; set; }

        //Currently inactive - Weapons of the ped
        public List<string> Weapons
        { get; set; }

        public Dictionary<int, int> Ammo
        { get; set; }

        //Currently inactive - Aim-Position of the Ped
        public Vector3 WeaponAimPos
        { get; set; }

        //Currently inactive - Current Task of the Ped with its params
        public string CurrentTask
        { get; set; }

        public List<string> CurrentTaskParams
        { get; set; }

        //Currently inactive - will contain information if the ped is never moving
        public bool Freeze
        { get; set; }

        //Currently inactive - Tells if the ped is randomly wandering
        //Caution: if the ped is not freezed, it will not wandering
        public bool Wandering
        { get; set; }

        /**
		 * If the Ped is Wandering, this tells the intermediate position
		 * of his wandering and also the final destination of his wandering.
		 *
		 * After the ped reached his final position a new route will be calculated.
		 */
        public List<NavigationMeshPolyFootpath> navmashPositions = new List<NavigationMeshPolyFootpath>();

        public List<NavigationMeshPolyFootpath> NavmashPositions
        {
            get
            {
                return navmashPositions;
            }
            set
            {
                if (value.Count == 0) return;
                this.navmashPositions = value;
                this.NearFinalPosition = false;
                Alt.EmitAllClients("pedSyncer:server:path", this.Id, this.NavmashPositions);
            }
        }

        public bool NearFinalPosition
        { get; set; }

        public int CurrentNavmashPositionsIndex
        { get; set; }

        /**
		 * Object Methods
		 */

        public Ped(float x, float y, float z, string model = null) : base(PED_TYPE, new Vector3(x, y, z), 0, STREAMING_RANGE)
        {
            peds[this.Id] = this;
            this.Valid = true;
            this.Heading = 0;
            this.Model = model;
            this.Invincible = false;
            this.Vehicle = null;
            this.Seat = 0;
            this.HasBlood = false;
            this.Health = 200;
            this.Armour = 0;
            this.Weapons = new List<string>();
            this.Ammo = new Dictionary<int, int>();
            this.CurrentTask = null;
            this.CurrentTaskParams = new List<string>();
            this.Freeze = false;
            this.Wandering = false;
            this.NearFinalPosition = false;
            this.CurrentNavmashPositionsIndex = 0;

            AltEntitySync.AddEntity(this);
            Alt.EmitAllClients("pedSyncer:server:create", this);
        }

        public void Destroy()
        {
            Alt.EmitAllClients("pedSyncer:server:delete", this.Id);
        }

        public void StartWandering(NavigationMeshPolyFootpath StartNavMesh = null)
        {
            NavigationMeshControl NavigationMeshControl = NavigationMeshControl.getInstance();

            if (StartNavMesh == null) StartNavMesh = NavigationMeshControl.getMeshByPosition(WorldVector3.ToWorldVector3(this.Position));

            this.NavmashPositions = NavigationMeshControl.getRandomPathByMesh(StartNavMesh);
        }

        public void ContinueWandering()
        {
            if (NavmashPositions.Count < 2) return;

            NavigationMeshControl NavigationMeshControl = NavigationMeshControl.getInstance();

            this.NavmashPositions = NavigationMeshControl.getRandomPathByMeshAndGon(
                this.NavmashPositions[this.NavmashPositions.Count - 1],
                WorldVector3.directionalAngle(this.NavmashPositions[this.NavmashPositions.Count - 1].Position, this.NavmashPositions[this.NavmashPositions.Count - 2].Position)
            );

            this.NearFinalPosition = false;
        }

        /**
		 * Class Methods
		 */

        public static List<Ped> All
        {
            get
            {
                return peds.Values.ToList<Ped>();
            }
        }

        public static Ped GetByID(ulong Id)
        {
            if (!peds.ContainsKey(Id)) return null;
            return peds[Id];
        }

        public static List<Ped> GetNear(Vector3 Position, float Distance)
        {
            List<Ped> NearPeds = new List<Ped>();
            foreach (Ped ped in peds.Values)
            {
                if (Utils.InDistanceBetweenPos(ped.Position, Position, Distance)) NearPeds.Add(ped);
            }

            return NearPeds;
        }

        public static void CreateCitizenPeds()
        {
            NavigationMeshControl NavigationMeshControl = NavigationMeshControl.getInstance();

            List<NavigationMeshPolyFootpath> RandomSpawns = NavigationMeshControl.getRandomSpawnMeshes();

            foreach (NavigationMeshPolyFootpath RandomSpawn in RandomSpawns)
            {
                Ped ped = new Ped(RandomSpawn.Position.X, RandomSpawn.Position.Y, RandomSpawn.Position.Z);
                ped.StartWandering(RandomSpawn);
            }
        }

        public void OnWrite(IMValueWriter writer)
        {
            writer.BeginObject();

            writer.Name("id");
            writer.Value(this.Id);

            writer.Name("dimension");
            writer.Value(this.Dimension);

            writer.Name("pos");
            writer.BeginObject();
            writer.Name("x");
            writer.Value(this.Position.X);
            writer.Name("y");
            writer.Value(this.Position.Y);
            writer.Name("z");
            writer.Value(this.Position.Z);
            writer.EndObject();

            writer.Name("netOwner");
            if (this.NetOwner != null) writer.Value(this.NetOwner.Id);
            else writer.Value("");

            writer.Name("valid");
            writer.Value(this.Valid);

            writer.Name("created");
            writer.Value(this.Created);

            writer.Name("heading");
            writer.Value(this.Heading);

            writer.Name("model");
            writer.Value(this.Model);

            writer.Name("drawable1");
            writer.Value(this.Drawable1);

            writer.Name("drawable2");
            writer.Value(this.Drawable2);

            writer.Name("drawable3");
            writer.Value(this.Drawable3);

            writer.Name("drawable4");
            writer.Value(this.Drawable4);

            writer.Name("drawable5");
            writer.Value(this.Drawable5);

            writer.Name("drawable6");
            writer.Value(this.Drawable6);

            writer.Name("drawable7");
            writer.Value(this.Drawable7);

            writer.Name("drawable8");
            writer.Value(this.Drawable8);

            writer.Name("drawable9");
            writer.Value(this.Drawable9);

            writer.Name("drawable10");
            writer.Value(this.Drawable10);

            writer.Name("drawable11");
            writer.Value(this.Drawable11);

            writer.Name("texture1");
            writer.Value(this.Texture1);

            writer.Name("texture2");
            writer.Value(this.Texture2);

            writer.Name("texture3");
            writer.Value(this.Texture3);

            writer.Name("texture4");
            writer.Value(this.Texture4);

            writer.Name("texture5");
            writer.Value(this.Texture5);

            writer.Name("texture6");
            writer.Value(this.Texture6);

            writer.Name("texture7");
            writer.Value(this.Texture7);

            writer.Name("texture8");
            writer.Value(this.Texture8);

            writer.Name("texture9");
            writer.Value(this.Texture9);

            writer.Name("texture10");
            writer.Value(this.Texture10);

            writer.Name("texture11");
            writer.Value(this.Texture11);

            writer.Name("prop0");
            writer.Value(this.Prop0);

            writer.Name("prop1");
            writer.Value(this.Prop1);

            writer.Name("prop2");
            writer.Value(this.Prop2);

            writer.Name("prop6");
            writer.Value(this.Prop6);

            writer.Name("prop7");
            writer.Value(this.Prop7);

            writer.Name("gender");
            writer.Value(this.Gender);

            writer.Name("invincible");
            writer.Value(this.Invincible);

            writer.Name("vehicle");
            writer.Value(this.Vehicle);

            writer.Name("seat");
            writer.Value(this.Seat);

            writer.Name("hasBlood");
            writer.Value(this.HasBlood);

            writer.Name("armour");
            writer.Value(this.Armour);

            writer.Name("health");
            writer.Value(this.Health);

            writer.Name("currentTask");
            writer.Value(this.CurrentTask);

            writer.Name("currentTaskParams");
            writer.BeginArray();
            foreach (string value in this.CurrentTaskParams) writer.Value(value);
            writer.EndArray();

            writer.Name("freeze");
            writer.Value(this.Freeze);

            writer.Name("wandering");
            writer.Value(this.Wandering);

            writer.Name("navmashPositions");
            writer.BeginArray();
            foreach (NavigationMeshPolyFootpath navMeshPos in this.NavmashPositions)
            {
                writer.BeginObject();
                writer.Name("x");
                writer.Value(navMeshPos.Position.X);
                writer.Name("y");
                writer.Value(navMeshPos.Position.Y);
                writer.Name("z");
                writer.Value(navMeshPos.Position.Z);
                writer.EndObject();
            }
            writer.EndArray();

            writer.Name("nearFinalPosition");
            writer.Value(this.NearFinalPosition);

            writer.Name("currentNavmashPositionsIndex");
            writer.Value(this.CurrentNavmashPositionsIndex);

            writer.EndObject();
        }
    }
}
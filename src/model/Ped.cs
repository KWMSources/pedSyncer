using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using PedSyncer.Control;
using PedSyncer.Model;
using PedSyncer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace PedSyncer.Model
{
    public class Ped : AltV.Net.EntitySync.Entity, IWritable
    {

        #region Properties
        public const ulong PED_TYPE = 1654;
        public const int STREAMING_RANGE = 200;
        #endregion

        //Directory to handle all peds
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
        {
            get
            {
                if (this.TryGetData<bool>("valid", out bool value)) return value;
                return true;
            }
            set
            {
                this.SetData("valid", value);
            }
        }

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
        {
            get
            {
                if (this.TryGetData<double>("heading", out double value)) return value;
                return 0;
            }
            set
            {
                this.SetData("heading", value);
            }
        }

        /**
		 * Ped-Style Properties
		 */

        public string Model
        {
            get
            {
                if (this.TryGetData<string>("model", out string value)) return value;
                return "";
            }
            set
            {
                this.SetData("model", value);
            }
        }

        public int Drawable1
        {
            get
            {
                if (this.TryGetData<int>("drawable1", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable1", value);
            }
        }

        public int Drawable2
        {
            get
            {
                if (this.TryGetData<int>("drawable2", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable2", value);
            }
        }

        public int Drawable3
        {
            get
            {
                if (this.TryGetData<int>("drawable3", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable3", value);
            }
        }

        public int Drawable4
        {
            get
            {
                if (this.TryGetData<int>("drawable4", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable4", value);
            }
        }

        public int Drawable5
        {
            get
            {
                if (this.TryGetData<int>("drawable5", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable5", value);
            }
        }

        public int Drawable6
        {
            get
            {
                if (this.TryGetData<int>("drawable6", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable6", value);
            }
        }

        public int Drawable7
        {
            get
            {
                if (this.TryGetData<int>("drawable7", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable7", value);
            }
        }

        public int Drawable8
        {
            get
            {
                if (this.TryGetData<int>("drawable8", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable8", value);
            }
        }

        public int Drawable9
        {
            get
            {
                if (this.TryGetData<int>("drawable9", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable9", value);
            }
        }

        public int Drawable10
        {
            get
            {
                if (this.TryGetData<int>("drawable10", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable10", value);
            }
        }

        public int Drawable11
        {
            get
            {
                if (this.TryGetData<int>("drawable11", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("drawable11", value);
            }
        }

        public int Texture1
        {
            get
            {
                if (this.TryGetData<int>("texture1", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture1", value);
            }
        }

        public int Texture2
        {
            get
            {
                if (this.TryGetData<int>("texture2", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture2", value);
            }
        }

        public int Texture3
        {
            get
            {
                if (this.TryGetData<int>("texture3", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture3", value);
            }
        }

        public int Texture4
        {
            get
            {
                if (this.TryGetData<int>("texture4", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture4", value);
            }
        }

        public int Texture5
        {
            get
            {
                if (this.TryGetData<int>("texture5", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture5", value);
            }
        }

        public int Texture6
        {
            get
            {
                if (this.TryGetData<int>("texture6", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture6", value);
            }
        }

        public int Texture7
        {
            get
            {
                if (this.TryGetData<int>("texture7", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture7", value);
            }
        }

        public int Texture8
        {
            get
            {
                if (this.TryGetData<int>("texture8", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture8", value);
            }
        }

        public int Texture9
        {
            get
            {
                if (this.TryGetData<int>("texture9", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture9", value);
            }
        }

        public int Texture10
        {
            get
            {
                if (this.TryGetData<int>("texture10", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture10", value);
            }
        }

        public int Texture11
        {
            get
            {
                if (this.TryGetData<int>("texture11", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("texture11", value);
            }
        }

        public int Prop0
        {
            get
            {
                if (this.TryGetData<int>("prop0", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("prop0", value);
            }
        }

        public int Prop1
        {
            get
            {
                if (this.TryGetData<int>("prop1", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("prop1", value);
            }
        }

        public int Prop2
        {
            get
            {
                if (this.TryGetData<int>("prop2", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("prop2", value);
            }
        }

        public int Prop6
        {
            get
            {
                if (this.TryGetData<int>("prop6", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("prop6", value);
            }
        }

        public int Prop7
        {
            get
            {
                if (this.TryGetData<int>("prop7", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("prop7", value);
            }
        }

        public string Gender
        { get; set; }

        //Ped Flags
        public bool[] Flags
        {
            get
            {
                if (this.TryGetData<bool[]>("flags", out bool[] value)) return value;
                return new bool[0];
            }
            set
            {
                this.SetData("flags", value);
            }
        }

        //Currently inactive - will contain information if the ped is invincible
        public bool Invincible
        {
            get
            {
                if (this.TryGetData<bool>("invincible", out bool value)) return value;
                return false;
            }
            set
            {
                this.SetData("invincible", value);
            }
        }

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
        {
            get
            {
                if (this.TryGetData<bool>("hasBlood", out bool value)) return value;
                return false;
            }
            set
            {
                this.SetData("hasBlood", value);
            }
        }

        //Property to set the armour of the ped
        public int Armour
        {
            get
            {
                if (this.TryGetData<int>("armour", out int value)) return value;
                return 0;
            }
            set
            {
                this.SetData("armour", value);
            }
        }

        //Property to set the health of the ped
        public int Health
        {
            get
            {
                if (this.TryGetData<int>("health", out int value)) return value;
                return 200;
            }
            set
            {
                this.SetData("health", value);
            }
        }

        //Property to set the death of the ped
        public bool Dead
        {
            get
            {
                if (this.TryGetData<bool>("dead", out bool value)) return value;
                return false;
            }
            set
            {
                this.SetData("dead", value);
            }
        }

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

        //Set the ped freezing and holding the position
        public bool Freeze
        {
            get
            {
                if (this.TryGetData<bool>("freeze", out bool value)) return value;
                return false;
            }
            set
            {
                this.SetData("freeze", value);
            }
        }

        //Currently inactive - Tells if the ped is randomly wandering
        //Caution: if the ped is not freezed, it will not wandering
        public bool Wandering
        {
            get
            {
                if (this.TryGetData<bool>("wandering", out bool value)) return value;
                return false;
            }
            set
            {
                this.SetData("wandering", value);
            }
        }

        /**
		 * If the Ped is Wandering, this tells the intermediate position
		 * of his wandering and also the final destination of his wandering.
		 *
		 * After the ped reached his final position a new route will be calculated.
		 */
        public List<IPathElement> pathPositions = new List<IPathElement>();

        public List<IPathElement> PathPositions
        {
            get
            {
                return pathPositions;
            }
            set
            {
                if (value.Count == 0) return;
                this.pathPositions = value;
                this.NearFinalPosition = false;
                Alt.EmitAllClients("pedSyncer:server:path", this.Id, this.PathPositions);
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
            this.Model = model;

            this.Vehicle = null;
            this.Seat = 0;

            this.Weapons = new List<string>();
            this.Ammo = new Dictionary<int, int>();

            this.CurrentTask = null;
            this.CurrentTaskParams = new List<string>();

            this.Flags = new bool[426];

            this.NearFinalPosition = false;
            this.CurrentNavmashPositionsIndex = 0;
            //AltAsync.Do(() =>
            //{
                AltEntitySync.AddEntity(this);
                Alt.EmitAllClients("pedSyncer:server:create", this);
            //});
        }

        public void Destroy()
        {      
            Alt.EmitAllClients("pedSyncer:server:delete", this.Id);
        }

        //Method to start the wandering of the ped
        public void StartWandering(IPathElement StartNavMesh = null)
        {
            NavigationMesh NavigationMeshControl = NavigationMesh.getInstance();

            if (StartNavMesh == null) StartNavMesh = NavigationMeshControl.getMeshByPosition(this.Position);

            //TODO: Invinite Loop
            if (StartNavMesh == null)
            {
                this.PathPositions = new List<IPathElement>();
                this.PathPositions.Add(NavigationMeshControl.getNearestMeshByPosition(this.Position));
                Console.WriteLine("Wandering at null: " + this.Id);
                return;
            }

            this.PathPositions = StartNavMesh.GetWanderingPath();
        }

        //Method to let the ped further wander at the moment the ped reaches the final destination
        public void ContinueWandering()
        {
            if (PathPositions.Count < 2) return;

            NavigationMesh NavigationMeshControl = NavigationMesh.getInstance();

            this.PathPositions = this.PathPositions[this.PathPositions.Count - 1].GetWanderingPathByDirection(
                Vector3Utils.directionalAngle(this.PathPositions[this.PathPositions.Count - 1].Position, this.PathPositions[this.PathPositions.Count - 2].Position)
            );

            this.NearFinalPosition = false;
        }

        //Method to set the flags
        public void SetFlags(bool[] flags) {
            this.Flags = flags;
        }

        //Property and Method to generate a random ped model
        private static Dictionary<int, List<int>> ModelsToNavMeshAreas = new Dictionary<int, List<int>>();

        public void SetRandomModel()
        {
            if (this.Model == "")
            {
                if (ModelsToNavMeshAreas.Count == 0) 
                    Ped.ModelsToNavMeshAreas = FileControl.LoadDataFromJsonFile<Dictionary<int, List<int>>>("resources/pedSyncer/ModelsToAreas.json");

                Random RandomKey = new Random();

                NavigationMeshPolyFootpath nearestNavMesh = NavigationMesh.getInstance().getNearestMeshByPosition(this.Position);

                this.Model = Ped.ModelsToNavMeshAreas[nearestNavMesh.AreaId][RandomKey.Next(0, Ped.ModelsToNavMeshAreas[nearestNavMesh.AreaId].Count - 1)].ToString();
            }
        }

        //Method to serialize this object
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
            foreach (IPathElement navMeshPos in this.PathPositions)
            {
                writer.BeginObject();
                writer.Name("x");
                writer.Value(navMeshPos.Position.X);
                writer.Name("y");
                writer.Value(navMeshPos.Position.Y);
                writer.Name("z");
                writer.Value(navMeshPos.Position.Z);
                writer.Name("streetCrossing");
                writer.Value(typeof(StreetCrossing).IsInstanceOfType(navMeshPos));
                writer.EndObject();
            }
            writer.EndArray();

            writer.Name("nearFinalPosition");
            writer.Value(this.NearFinalPosition);

            writer.Name("currentNavmashPositionsIndex");
            writer.Value(this.CurrentNavmashPositionsIndex);

            writer.EndObject();
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
                if (Vector3Utils.InDistanceBetweenPos(ped.Position, Position, Distance)) NearPeds.Add(ped);
            }

            return NearPeds;
        }

        //Method to generate wandering peds as citizens
        public static void CreateCitizenPeds()
        {
            NavigationMesh NavigationMeshControl = NavigationMesh.getInstance();

            //Load random navMeshes to spawn peds on it
            List<NavigationMeshPolyFootpath> RandomSpawnsList = NavigationMeshControl.getRandomSpawnMeshes();

            //Spawn the peds on these navMeshes and let the ped wander
            Parallel.ForEach(RandomSpawnsList, RandomSpawn =>
            {
                Ped ped = new Ped(RandomSpawn.Position.X, RandomSpawn.Position.Y, RandomSpawn.Position.Z);
                ped.SetRandomModel();
                ped.StartWandering(RandomSpawn);
            });
        }
    }
}
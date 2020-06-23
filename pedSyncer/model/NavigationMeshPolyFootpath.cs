using System;
using System.Collections.Generic;
using AltV.Net;
using MessagePack;

namespace NavMesh_Graph
{
    [MessagePackObject]
    public class NavigationMeshPolyFootpath: IWritable
    {
        [Key(0)]
        public int Index { get; set; }

        [Key(1)]
        public int PartId { get; set; }

        [Key(2)]
        public WorldVector3 Position { get; set; }

        [Key(3)]
        public List<WorldVector3> Vertices { get; set; }

        [Key(4)]
        public int Id { get; set; }

        [Key(5)]
        public int AreaId { get; set; }

        [Key(6)]
        public List<int> Neighbours { get; set; }

        [IgnoreMember]
        public List<NavigationMeshPolyFootpath> NeighboursObjects { get; set; }

        [SerializationConstructor]
        public NavigationMeshPolyFootpath()
        {
        }

        public static Dictionary<(int, int), List<NavigationMeshPolyFootpath>> navMeshesMap = new Dictionary<(int, int), List<NavigationMeshPolyFootpath>>();


        public static bool isNeighbour(NavigationMeshPolyFootpath poly1, NavigationMeshPolyFootpath poly2)
        {
            foreach (WorldVector3 vec1 in poly1.Vertices)
            {
                foreach (WorldVector3 vec2 in poly2.Vertices)
                {
                    if (WorldVector3.@equals(vec1, vec2)) return true;
                }
            }

            return false;
		}
        public void OnWrite(IMValueWriter writer)
        {
            writer.BeginObject();
            writer.Name("x");
            writer.Value(this.Position.X);
            writer.Name("y");
            writer.Value(this.Position.Y);
            writer.Name("z");
            writer.Value(this.Position.Z);
            writer.EndObject();
        }
	}
}
using AltV.Net;
using MessagePack;
using pedSyncer.control;
using pedSyncer.model;
using System;
using System.Collections.Generic;

namespace NavMesh_Graph
{
    [MessagePackObject]
    public class NavigationMeshPolyFootpath : IPathElement
    {
        [Key(0)]
        public int Index { get; set; }

        [Key(1)]
        public int PartId { get; set; }

        [Key(3)]
        public List<WorldVector3> Vertices { get; set; }

        [Key(4)]
        public int Id { get; set; }

        [Key(5)]
        public int AreaId { get; set; }

        [Key(6)]
        public List<int> Neighbours { get; set; }

        [Key(7)]
        public List<WorldVector3> StreetCrossings { get; set; }

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

        public override List<IPathElement> GetNeighbours()
        {
            List<IPathElement> pathElementList = new List<IPathElement>();

            if (this.NeighboursObjects != null) 
            {
                foreach (NavigationMeshPolyFootpath navigationMeshPolyFootpath in this.NeighboursObjects)
                {
                    pathElementList.Add(navigationMeshPolyFootpath);
                }
            }

            if (this.StreetCrossings != null)
            {
                foreach (WorldVector3 streetCrossing in this.StreetCrossings)
                {
                    if (!StreetCrossingControl.MappedStreetCrossings.ContainsKey(streetCrossing.ToString())) continue;
                    pathElementList.Add(StreetCrossingControl.MappedStreetCrossings[streetCrossing.ToString()]);
                }
            }

            return pathElementList;
        }
    }
}
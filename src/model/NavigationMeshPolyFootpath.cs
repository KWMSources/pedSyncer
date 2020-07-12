using AltV.Net;
using MessagePack;
using PedSyncer.Control;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PedSyncer.Model
{
    [MessagePackObject]
    public class NavigationMeshPolyFootpath : IPathElement
    {
        [Key(0)]
        public int Index { get; set; }

        [Key(1)]
        public int PartId { get; set; }

        [Key(3)]
        public List<WorldVector3> VerticesTemp 
        { 
            set {
                this.Vertices = new List<Vector3>();

                if (value == null) return;
                foreach (WorldVector3 worldVector3 in value)
                    this.Vertices.Add(worldVector3.ToVector3());
            } 
        }

        [IgnoreMember]
        public List<Vector3> Vertices { get; set; }

        [Key(4)]
        public int Id { get; set; }

        [Key(5)]
        public int AreaId { get; set; }

        [Key(6)]
        public List<int> Neighbours { get; set; }

        [Key(7)]
        public List<WorldVector3> StreetCrossingsTemp
        {
            set
            {
                this.StreetCrossings = new List<Vector3>();

                if (value == null) return;
                foreach (WorldVector3 worldVector3 in value)
                    this.StreetCrossings.Add(worldVector3.ToVector3());
            }
        }

        [IgnoreMember]
        public List<Vector3> StreetCrossings { get; set; }

        [IgnoreMember]
        public List<NavigationMeshPolyFootpath> NeighboursObjects { get; set; }

        [SerializationConstructor]
        public NavigationMeshPolyFootpath()
        {
        }

        //Method to check if two navMeshes are neighbours
        public static bool isNeighbour(NavigationMeshPolyFootpath poly1, NavigationMeshPolyFootpath poly2)
        {
            foreach (Vector3 vec1 in poly1.Vertices)
            {
                foreach (Vector3 vec2 in poly2.Vertices)
                {
                    if (Vector3.Equals(vec1, vec2)) return true;
                }
            }

            return false;
        }

        //Get neighbours of this navMesh as IPathElement
        public override List<IPathElement> GetNeighbours()
        {
            List<IPathElement> pathElementList = new List<IPathElement>();

            //Collect all navMesh neighbours
            if (this.NeighboursObjects != null) 
            {
                foreach (NavigationMeshPolyFootpath navigationMeshPolyFootpath in this.NeighboursObjects)
                {
                    pathElementList.Add(navigationMeshPolyFootpath);
                }
            }

            //Collect all streetCrossings neighbours
            if (this.StreetCrossings != null)
            {
                foreach (Vector3 streetCrossing in this.StreetCrossings)
                {
                    if (!StreetCrossingControl.MappedStreetCrossings.ContainsKey(streetCrossing.ToString())) continue;
                    pathElementList.Add(StreetCrossingControl.MappedStreetCrossings[streetCrossing.ToString()]);
                }
            }

            return pathElementList;
        }
    }
}
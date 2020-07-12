using MessagePack;
using PedSyncer.Control;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PedSyncer.Model
{
    /**
     * 
     * StreetCrossing elements
     * 
     * StreetCrossings are positions connecting with other positions (as 
     * navMeshes or other StreetCrossings) going over streets
     * 
     */

    [MessagePackObject]
    public class StreetCrossing: IPathElement
    {
        [Key(1)]
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

        [Key(2)]
        public List<WorldVector3> NavMeshesTemp
        {
            set
            {
                this.NavMeshes = new List<Vector3>();

                if (value == null) return;
                foreach (WorldVector3 worldVector3 in value)
                    this.NavMeshes.Add(worldVector3.ToVector3());
            }
        }

        [IgnoreMember]
        public List<Vector3> NavMeshes { get; set; }

        [SerializationConstructor]
        public StreetCrossing()
        {
        }

        //Get neighbours of this streetcrossing as IPathElement
        public override List<IPathElement> GetNeighbours()
        {
            List<IPathElement> pathElementList = new List<IPathElement>();
            NavigationMesh navigationMeshControl = NavigationMesh.getInstance();

            //Collect all navMesh neighbours
            foreach (Vector3 navigationMeshPolyFootpath in this.NavMeshes)
            {
                pathElementList.Add(navigationMeshControl.getMeshByPosition(navigationMeshPolyFootpath));
            }

            //Collect all streetCrossings neighbours
            foreach (Vector3 streetCrossing in this.StreetCrossings)
            {
                pathElementList.Add(StreetCrossingControl.MappedStreetCrossings[streetCrossing.ToString()]);
            }

            return pathElementList;
        }
    }
}

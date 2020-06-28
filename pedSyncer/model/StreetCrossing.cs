using MessagePack;
using NavMesh_Graph;
using navMesh_Graph_WebAPI;
using pedSyncer.control;
using System;
using System.Collections.Generic;
using System.Text;

namespace pedSyncer.model
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
        public List<WorldVector3> StreetCrossings { get; set; }

        [Key(2)]
        public List<WorldVector3> NavMeshes { get; set; }

        [SerializationConstructor]
        public StreetCrossing()
        {
        }

        //Get neighbours of this streetcrossing as IPathElement
        public override List<IPathElement> GetNeighbours()
        {
            List<IPathElement> pathElementList = new List<IPathElement>();
            NavigationMeshControl navigationMeshControl = NavigationMeshControl.getInstance();

            //Collect all navMesh neighbours
            foreach (WorldVector3 navigationMeshPolyFootpath in this.NavMeshes)
            {
                pathElementList.Add(navigationMeshControl.getMeshByPosition(navigationMeshPolyFootpath));
            }

            //Collect all streetCrossings neighbours
            foreach (WorldVector3 streetCrossing in this.StreetCrossings)
            {
                pathElementList.Add(StreetCrossingControl.MappedStreetCrossings[streetCrossing.ToString()]);
            }

            return pathElementList;
        }
    }
}

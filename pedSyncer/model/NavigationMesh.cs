using System.Collections.Generic;
using MessagePack;

namespace NavMesh_Graph
{
    [MessagePackObject]
    public class NavigationMesh
    {
        [Key(0)]
        public int AreaId { get; set; }

        [Key(1)]
        public int CellX { get; set; }

        [Key(2)]
        public int CellY { get; set; }

        [Key(3)]
        public List<NavigationMeshPoly> Polygons { get; set; }
    }
}
using MessagePack;

namespace NavMesh_Graph
{
    [MessagePackObject]
    public class NavigationMeshPolyEdge
    {
        [Key(0)]
        public uint AreaId { get; set; }

        [Key(1)]
        public uint PolyIndex { get; set; }
    }
}
using System.Collections.Generic;
using System.Numerics;

namespace PedSyncer.Model
{
    public class StreetNodeArea
    {
        public int AreaId { get; set; }
        public int CellX { get; set; }
        public int CellY { get; set; }
        public Vector3 DimensionMin { get; set; }
        public Vector3 DimensionMax { get; set; }
        public List<StreetNode> Nodes { get; set; }
    }
}
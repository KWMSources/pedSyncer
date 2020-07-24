using System.Collections.Generic;
using System.Numerics;

namespace PedSyncer.Model
{
    public class StreetNode
    {
        public int Id { get; set; }
        public string StreetName { get; set; }
        public bool IsValidForGps { get; set; }
        public bool IsJunction { get; set; }
        public bool IsFreeway { get; set; }
        public bool IsGravelRoad { get; set; }
        public bool IsBackroad { get; set; }
        public bool IsOnWater { get; set; }
        public bool IsPedCrossway { get; set; }
        public bool TrafficlightExists { get; set; }
        public bool LeftTurnNoReturn { get; set; }
        public bool RightTurnNoReturn { get; set; }
        public Vector3 Position { get; set; }
        public List<StreetNodeConnected> ConnectedNodes { get; set; }
    }
}
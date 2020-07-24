namespace PedSyncer.Model
{
    public class StreetNodeConnected
    {
        public StreetNode Node { get; set; }
        public int LaneCountForward { get; set; }
        public int LaneCountBackward { get; set; }
    }
}
using System.Collections.Generic;

namespace GeoDatabase
{
    public class GeoEntity
    {
        public long Id { get; set; }
    }
    public class GeoNode : GeoEntity
    {
        public GeoNode()
        {
            NeighbourNodes = new List<NeighbourNode>();
        }
        public long OSMId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public virtual List<NeighbourNode> NeighbourNodes { get; set; }
    }
    public class NeighbourNode
    {
        public long NodeId { get; set; }
        public long OSMId { get; set; }
        public double Distance { get; set; }
    }
}

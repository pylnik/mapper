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
            NeighbourNodes = new List<GeoNode>();
        }
        public long OSMId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public virtual List<GeoNode> NeighbourNodes { get; set; }
    }
}

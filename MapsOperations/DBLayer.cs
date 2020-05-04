using GeoDatabase;
using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MapsOperations
{
    public class DBLayer : IDisposable
    {
        LiteDBDriver _liteDB;
        public void Dispose()
        {
        }
        public DBLayer(string dataBasePath)
        {
            _liteDB = new LiteDBDriver(dataBasePath);
        }
        public List<GeoNode> GetBoundNodes()
        {
            return _liteDB.GetAllNodes().Where(nd => nd.NeighbourNodes.Count > 0).ToList();
        }
        public void ImportNodes(OsmStreamSource source, bool forceIfTableEmpty = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (_liteDB is null)
                throw new ArgumentNullException(nameof(_liteDB));

            if (_liteDB.GetAllNodes().Count() != 0 || !forceIfTableEmpty)
                return;
            var allNodes = from osmGeo in source
                           where
                           osmGeo.Type == OsmGeoType.Node
                           let nd = ((Node)osmGeo)
                           where nd.Id != null && nd.Latitude != null && nd.Longitude != null
                           select new GeoNode { OSMId = osmGeo.Id.Value, Latitude = nd.Latitude.Value, Longitude = nd.Longitude.Value };
            _liteDB.InsertNodes(allNodes);
        }

        public void ImportWays(OsmStreamSource source, bool forceIfTableEmpty = false)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (_liteDB is null)
                throw new ArgumentNullException(nameof(_liteDB));
            var filteredWays = from osmGeo in source
                               where
                               osmGeo.Type == OsmGeoType.Way
                               let way = (Way)osmGeo
                               where
                               osmGeo.Tags.Contains("highway", "secondary")
                               || osmGeo.Tags.Contains("highway", "tertiary")
                               || osmGeo.Tags.Contains("highway", "unclassified")
                               || osmGeo.Tags.Contains("highway", "residential")
                               || osmGeo.Tags.Contains("highway", "service")
                               || osmGeo.Tags.Contains("highway", "cycleway")
                               || osmGeo.Tags.ContainsKey("footway")
                               || osmGeo.Tags.Contains("sidewalk", "both")
                               || osmGeo.Tags.Contains("sidewalk", "left")
                               || osmGeo.Tags.Contains("sidewalk", "right")
                               //let nodes = from nd in way.Nodes
                               //            select
                               //            _liteDB.GetNodeByOSMID(nd)
                               select
                               new LinkedList<long>(way.Nodes)
               ;
            foreach (var nodes in filteredWays)
            {
                var curNode = nodes.First;
                while (curNode != null)
                {
                    var nextNode = curNode.Next;
                    if (nextNode == null)
                        break;
                    _liteDB.AddNodeToNeightbours(curNode.Value, nextNode.Value);
                    curNode = nextNode;
                }
            }
        }
    }
}

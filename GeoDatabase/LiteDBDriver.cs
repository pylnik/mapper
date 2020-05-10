using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GeoDatabase
{
    public class LiteDBDriver : IDisposable
    {
        LiteDatabase _liteDB;
        public string FileName { get; }
        public LiteDBDriver(string fileName)
        {
            InitMapper();
            FileName = fileName;
            _liteDB = new LiteDatabase(FileName);

            EnsureIndexes();
        }
        private static void InitMapper()
        {
            var mapper = BsonMapper.Global;
            mapper.Entity<GeoNode>()
                .DbRef(x => x.NeighbourNodes, "GeoNode");
        }
        private void EnsureIndexes()
        {
            var col = GetNodesCollection(_liteDB);
            col.EnsureIndex(n => n.OSMId);
        }
        public void DeleteNodes(IEnumerable<GeoNode> nodes)
        {
            var col = GetNodesCollection(_liteDB);
            foreach (var node in nodes)
            {
                col.Delete(node);
            }
        }
        public IEnumerable<GeoNode> GetAllNodes()
        {
            var col = GetNodesCollection(_liteDB);
            return col.Query()
                .Where(n => n.NeighbourNodes.Count > 0)
                .Include(x => x.NeighbourNodes)
                .ToList();
        }
        public GeoNode GetNodeByOSMID(long osmId)
        {
            var col = GetNodesCollection(_liteDB);
            return col.FindOne(n => n.OSMId == osmId);
        }
        public void InsertNodes(IEnumerable<GeoNode> nodes)
        {
            var col = GetNodesCollection(_liteDB);
            col.InsertBulk(nodes);
        }
        public void UpsertNode(GeoNode node)
        {
            var col = GetNodesCollection(_liteDB);
            col.Upsert(node);
        }
        public List<GeoNode> GetNeighbourNodes(GeoNode start)
        {
            var col = GetNodesCollection(_liteDB);
            var results =
                col.Query()
                .Where(n => start.NeighbourNodes.Exists(g => g.Id == n.Id))
                .Include(x => x.NeighbourNodes)
                .ToList();
            return results;
        }

        public void AddNodeToNeightbours(long osmNode1, long osmNode2)
        {
            var geoNode1 = GetNodeByOSMID(osmNode1);
            if (geoNode1 == null)
            {
                Trace.WriteLine($"Unable to find node with OSM id {osmNode1}");
                return;
            }
            var geoNode2 = GetNodeByOSMID(osmNode2);
            if (geoNode2 == null)
            {
                Trace.WriteLine($"Unable to find node with OSM id {osmNode1}");
                return;
            }
            if (geoNode1.NeighbourNodes.FirstOrDefault(n => n.OSMId == osmNode2) == null)
            {
                geoNode1.NeighbourNodes.Add(geoNode2);
                UpsertNode(geoNode1);
            }
            if (geoNode2.NeighbourNodes.FirstOrDefault(n => n.OSMId == osmNode1) == null)
            {
                geoNode2.NeighbourNodes.Add(geoNode1);
                UpsertNode(geoNode2);
            }
        }

        public void CreateDB()
        {
            ILiteCollection<GeoNode> col = GetNodesCollection(_liteDB);

            var node1 = new GeoNode { Latitude = 1, Longitude = 1 };
            var node2 = new GeoNode { Latitude = 2, Longitude = 2 };
            var node3 = new GeoNode { Latitude = 3, Longitude = 3 };

            col.Upsert(node1);
            col.Upsert(node2);
            col.Upsert(node3);

            node1.NeighbourNodes.Add(node2);
            node1.NeighbourNodes.Add(node3);
            col.Upsert(node1);
            node2.NeighbourNodes.Add(node1);
            col.Upsert(node2);
            node3.NeighbourNodes.Add(node1);
            col.Upsert(node3);


            //// Index document using document Name property
            //col.EnsureIndex(x => x.);

            // Use LINQ to query documents (filter, sort, transform)
            var results = col.Query()
                .Where(x => x.NeighbourNodes.Count > 0)
                .Include(x => x.NeighbourNodes)
                .ToList();

            Trace.TraceInformation($"Result = {results.Count}");
        }

        private static ILiteCollection<GeoNode> GetNodesCollection(LiteDatabase db)
        {
            return db.GetCollection<GeoNode>();
        }

        public void Dispose()
        {
            _liteDB?.Dispose();
        }
    }
}

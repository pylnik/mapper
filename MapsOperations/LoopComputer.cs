using GeoDatabase;
using NetTopologySuite.GeometriesGraph;
using osm_importer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MapsOperations
{
    public class LoopComputer
    {
        public float MinLength { get; set; }
        float _minHalfLength;
        Dictionary<long, GeoNode> _nodesCache;
        public DBLayer Dal { get; set; }
        List<GeoPath> HalfPaths { get; set; }
        public event Action<Tuple<GeoPath, GeoPath>> NewLoop;
        public void Compute()
        {
            if (Dal == null)
                throw new NotImplementedException(nameof(Dal));
            _minHalfLength = MinLength / 2;
            _nodesCache = Dal.GetAllNodesDictionary();
            var firstNode = _nodesCache.Values.First();
            HalfPaths = new List<GeoPath>();
            //var nodes = Dal.GetAllNodes().ToList();
            BuildTree(new List<GeoNode> { firstNode });
            var loops = GetLoops(HalfPaths);
            Trace.TraceInformation(loops.Count.ToString());
        }
        private List<Tuple<GeoPath, GeoPath>> GetLoops(List<GeoPath> halfLoops)
        {
            var loops = new List<Tuple<GeoPath, GeoPath>>();
            for (int i = 0; i < halfLoops.Count; i++)
            {
                var halfLoop = halfLoops[i];
                var lastNode = halfLoop.Nodes.Last();
                var lastId = lastNode.Id;
                for (int j = i + 1; j < halfLoops.Count; j++)
                {
                    var half2 = halfLoops[j];
                    if (half2.Nodes.Last().Id == lastId)
                    {
                        loops.Add(new Tuple<GeoPath, GeoPath>(halfLoop, half2));
                        NewLoop?.Invoke(loops.Last());
                    }
                }
            }
            return loops;
        }
        private void BuildTree(List<GeoNode> nodesToVisit)
        {
            foreach (var node in nodesToVisit)
            {
                TraceNodes(new List<GeoNode> { node }, 0);
            }
        }

        private void TraceNodes(List<GeoNode> visitedNodes, double curLength)
        {
            if (curLength > _minHalfLength)
            {
                HalfPaths.Add(new GeoPath { Nodes = visitedNodes, Length = (float)curLength });
                return;
            }
            GeoNode nodeFrom = visitedNodes.Last();
            var nodesToVisit = nodeFrom?.NeighbourNodes;
            foreach (var nodeTo in nodesToVisit)
            {
                if (visitedNodes.FirstOrDefault(n => n.Id == nodeTo.NodeId) == null)
                {
                    var nodeToFull = _nodesCache[nodeTo.NodeId];
                    var newVisitedNodes = visitedNodes.ToList();
                    newVisitedNodes.Add(nodeToFull);
                    var dist = nodeTo.Distance;
                    TraceNodes(newVisitedNodes, curLength + dist);
                }
            }
        }
        private double GetDistanceBetweenNodes(GeoNode node1, GeoNode node2)
        {
            return Haversine.Calculate(node1.Latitude, node1.Longitude, node2.Latitude, node2.Longitude);
        }
    }

    public class GeoPath
    {
        public List<GeoNode> Nodes { get; set; } = new List<GeoNode>();
        public float Length { get; set; }
    }
}

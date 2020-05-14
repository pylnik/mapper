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
        IEnumerable<GeoNode> _nodesCache;
        public DBLayer Dal { get; set; }
        public void Compute()
        {
            if (Dal == null)
                throw new NotImplementedException(nameof(Dal));
            _minHalfLength = MinLength / 2;
            _nodesCache = Dal.GetAllNodes().ToList();
            var firstNode = _nodesCache.First();
            //var nodes = Dal.GetAllNodes().ToList();
            var paths = GetPaths(new List<GeoNode> { firstNode });
            var loops = GetLoops(paths);
            Trace.TraceInformation(loops.Count.ToString());
        }
        private List<Tuple<GeoPath, GeoPath>> GetLoops(List<GeoPath> halfLoops)
        {
            var loops = new List<Tuple<GeoPath, GeoPath>>();
            IEnumerable<GeoPath> tail = halfLoops.ToList();
            foreach (var halfLoop in halfLoops)
            {
                var lastNode = halfLoop.Nodes.Last();
                var lastId = lastNode.Id;
                tail = tail.SkipWhile(p => p == halfLoop);
                foreach (var half2 in tail)
                {
                    if (half2.Nodes.Last().Id == lastId)
                        loops.Add(new Tuple<GeoPath, GeoPath>(halfLoop, half2));
                }
            }
            return loops;
        }
        private List<GeoPath> GetPaths(List<GeoNode> nodesToVisit)
        {
            List<GeoPath> paths = new List<GeoPath>();
            foreach (var node in nodesToVisit)
            {
                paths.AddRange(TraceNodes(new List<GeoNode> { node }, 0));
            }
            return paths;
        }

        private List<GeoPath> TraceNodes(List<GeoNode> visitedNodes, double curLength)
        {
            var nodes = new List<GeoPath>();
            if (curLength > _minHalfLength)
            {
                nodes.Add(new GeoPath { Nodes = visitedNodes, Length = (float)curLength });
                return nodes;
            }
            GeoNode nodeFrom = visitedNodes.Last();
            var nodesToVisit = nodeFrom?.NeighbourNodes;
            foreach (var nodeTo in nodesToVisit)
            {
                if (visitedNodes.FirstOrDefault(n => n.Id == nodeTo.NodeId) == null)
                {
                    var nodeToFull = _nodesCache.FirstOrDefault(n => n.Id == nodeTo.NodeId);
                    var newVisitedNodes = visitedNodes.ToList();
                    newVisitedNodes.Add(nodeToFull);
                    var dist = nodeTo.Distance;// GetDistanceBetweenNodes(nodeFrom, nodeToFull);
                    nodes.AddRange(TraceNodes(newVisitedNodes, curLength + dist));
                }
            }
            return nodes;
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

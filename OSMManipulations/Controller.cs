using MapsOperations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using OsmSharp.Streams;
using Itinero;
using GeoDatabase;
using osm_importer;

namespace OSMManipulations
{
    class Controller
    {
        private string PDBFileName = GetFullFileName(@"saarland-latest.osm.pbf");
        private const string ShapeFileName = "ways.shp";
        string MapperDBFileName = GetFullFileName(@"mapper.geodb");
        static string WorkingDirectory = @"F:\Mapper\";
        string RouteDbFileName = GetFullFileName(@"ways.routedb");
        string RouterDbAreaFileName = GetFullFileName(@"area.routedb");
        DBLayer _dBLayer;
        OSMOperations _osmOperations = new OSMOperations();
        MapBoundingBox _areaOfInterest = new MapBoundingBox { MinLat = 49.2068, MaxLat = 49.2579, MinLon = 6.9097, MaxLon = 7.0445 };
        public async Task Do()
        {
#warning works
            if (!File.Exists(MapperDBFileName))
                using (_dBLayer = new DBLayer(MapperDBFileName))
                    await Task.Run(() => _osmOperations.ImportOSM(PDBFileName, _dBLayer, mapBoundingBox: _areaOfInterest));

            using (_dBLayer = new DBLayer(MapperDBFileName))
            {
#warning works
                //_osmOperations.SaveDBToShape(_dBLayer, GetFullFileName("test.shp"));
#warning
                var nodesCache = _dBLayer.GetAllNodesDictionary();
                LoopComputer loopComputer = new LoopComputer { MinLength = 2, Dal = _dBLayer };
                loopComputer.NewLoop += (loop) => LoopComputer_NewLoop(nodesCache, loop);
                loopComputer.Compute();
            }
        }

        private void LoopComputer_NewLoop(Dictionary<long, GeoNode> allNodesCache, Tuple<GeoPath, GeoPath> obj)
        {
            var listToSave = new List<GeoNode>(obj.Item1.Nodes.Count + obj.Item2.Nodes.Count);
            listToSave.AddRange(obj.Item1.Nodes);
            var tail = obj.Item2.Nodes.ToList();
            tail.Reverse();
            listToSave.AddRange(tail);
            var fName = obj.GetHashCode().ToString();
            _osmOperations.SaveNodesToShape(GetFullFileName($@"loops\{fName}.shp"), allNodesCache, listToSave);
        }

        private static string GetFullFileName(string localFileName)
        {
            return Path.Combine(WorkingDirectory, localFileName);
        }
    }
}

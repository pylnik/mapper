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
            //var liteDb = new LiteDBDriver(MapperDBFileName);
            using (_dBLayer = new DBLayer(MapperDBFileName))
            {

                _osmOperations.SaveToShape(_dBLayer, GetFullFileName("test.shp"));
                //var db = _dBLayer.GeoContext;
                //{
                //    var nodesCount = db.Nodes.Count();
                //    var waysCount = db.Ways.Count();
                //    Console.WriteLine($"DB contains {nodesCount} nodes and {waysCount} ways");
                //}
                //return;
                //Console.WriteLine($"Loading area from '{RouterDbAreaFileName}'");
                //var area = await Task.Run(() => _osmOperations.LoadRouteDbFromFile(RouterDbAreaFileName));
                //if (area == null)
                //{
                //    Console.WriteLine($"Area == null -> extract from big RouterDB '{RouteDbFileName}'");
                //    var db = await Task.Run(() => _osmOperations.LoadRouteDbFromFile(RouteDbFileName));
                //    if (db == null)
                //    {
                //        Console.WriteLine($"RouterDB is null -> create from PBF-file '{PDBFileName}'");
                //        db = await Task.Run(() => _osmOperations.ImportWays(PDBFileName, RouteDbFileName));
                //        Console.WriteLine($"RouterDB extracted");
                //    }

                //    area = await Task.Run(() => _osmOperations.ExtractAreaFromDb(db, 49.24f, 6.95f, 49.19f, 7.04f));
                //    Console.WriteLine($"Area extracted");
                //    await Task.Run(() => _osmOperations.SaveRouterDbToFile(area, RouterDbAreaFileName));
                //    Console.WriteLine($"Area saved to '{RouterDbAreaFileName}'");
                //}



                //Console.WriteLine($"Save area to '{ShapeFileName}'");
                //await Task.Run(() => _osmOperations.SaveToShapeFile(area, GetFullFileName(ShapeFileName)));
                //Console.WriteLine($"Shape file saved");
                //LoopComputer loopComputer = new LoopComputer();
                //loopComputer.Compute(area);
                //using (var sw = new StreamWriter(File.Create(GetFullFileName("test.json"))))
                //{
                //    area.WriteGeoJson(sw);
                //}
            }
        }

        private static string GetFullFileName(string localFileName)
        {
            return Path.Combine(WorkingDirectory, localFileName);
        }
    }
}

using MapsOperations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OSMManipulations
{
    class Controller
    {
        private const string PDBFileName = @"D:\Downloads\saarland-latest.osm.pbf";
        private const string ShapeFileName = "ways.shp";
        static string WorkingDirectory = @"D:\Mapper\";
        string RouteDbFileName = GetFullFileName(@"ways.routedb");
        string RouterDbAreaFileName = GetFullFileName(@"area.routedb");

        OSMOperations _osmOperations = new OSMOperations();
        public async Task Do()
        {
            Console.WriteLine($"Loading area from '{RouterDbAreaFileName}'");
            var area = await Task.Run(() => _osmOperations.LoadRouteDbFromFile(RouterDbAreaFileName));
            if (area == null)
            {
                Console.WriteLine($"Area == null -> extract from big RouterDB '{RouteDbFileName}'");
                var db = await Task.Run(() => _osmOperations.LoadRouteDbFromFile(RouteDbFileName));
                if (db == null)
                {
                    Console.WriteLine($"RouterDB is null -> create from PBF-file '{PDBFileName}'");
                    db = await Task.Run(() => _osmOperations.ImportWays(PDBFileName, RouteDbFileName));
                    Console.WriteLine($"RouterDB extracted");
                }

                area = await Task.Run(() => _osmOperations.ExtractAreaFromDb(db, 49.24f, 6.95f, 49.19f, 7.04f));
                Console.WriteLine($"Area extracted");
                await Task.Run(() => _osmOperations.SaveRouterDbToFile(area, RouterDbAreaFileName));
                Console.WriteLine($"Area saved to '{RouterDbAreaFileName}'");
            }
            Console.WriteLine($"Save area to '{ShapeFileName}'");
            await Task.Run(() => _osmOperations.SaveToShapeFile(area, GetFullFileName(ShapeFileName)));
            Console.WriteLine($"Shape file saved");
        }

        private static string GetFullFileName(string localFileName)
        {
            return Path.Combine(WorkingDirectory, localFileName);
        }
    }
}

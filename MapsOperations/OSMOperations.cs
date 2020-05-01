using Itinero;
using Itinero.IO.Osm;
using Itinero.IO.Shape;
using Itinero.Osm.Vehicles;
using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MapsOperations
{
    public class OSMOperations
    {
        public RouterDb ExtractAreaFromDb(RouterDb routerDb, float minLat, float minLon, float maxLat, float maxLon)
        {
            return routerDb.ExtractArea(minLat, minLon, maxLat, maxLon);
        }
        public void SaveToShapeFile(RouterDb routerDb, string shapeFileName)
        {
            routerDb.WriteToShape(shapeFileName);
        }
        public RouterDb LoadRouteDbFromFile(string fileName)
        {
            try
            {
                using (var fs = File.OpenRead(fileName))
                {
                    return RouterDb.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Unable to load RouteDb from {fileName}. Exception {ex.Message}");
                return null;
            }
        }
        public RouterDb ImportWays(string pathToXML, string importToPath)
        {
            var routerDb = new RouterDb();
            using (var stream = new FileInfo(pathToXML).OpenRead())
            {
                // create the network for cars only.
                routerDb.LoadOsmData(stream, Vehicle.Bicycle, Vehicle.Pedestrian);
            }

            SaveRouterDbToFile(routerDb, importToPath);
            return routerDb;
        }

        public void SaveRouterDbToFile(RouterDb routerDb, string importToPath)
        {
            // write the routerdb to disk.
            using (var stream = File.Create(importToPath))
            {
                routerDb.Serialize(stream);
            }
        }

        public void ImportOSM(string pathToXML, string importToPath)
        {
            using (var fileStream = File.OpenRead(pathToXML))
            {
                // create source stream.
                //var source = new PBFOsmStreamSource(fileStream);
                var source = new XmlOsmStreamSource(fileStream);
                // filter all powerlines and keep all nodes.
                var filtered = from osmGeo in source
                               where
                                    osmGeo.Type == OsmSharp.OsmGeoType.Node
                                    && osmGeo.Tags != null
                                    && osmGeo.Tags.Contains("highway", "crossing")
                                    && osmGeo is Node
                                    && ((Node)osmGeo).Longitude.HasValue
                                    && ((Node)osmGeo).Latitude.HasValue
                               select new PointF((float)((Node)osmGeo).Latitude, (float)((Node)osmGeo).Longitude);

                var Flist = filtered.ToList();

                using (var sw = File.CreateText(importToPath))
                {
                    foreach (var coord in Flist)
                    {
                        sw.WriteLine($"{coord.X} {coord.Y}");
                    }
                }

                // convert to complete stream.
                // WARNING: nodes that are partof powerlines will be kept in-memory.
                //          it's important to filter only the objects you need **before** 
                //          you convert to a complete stream otherwise all objects will 
                //          be kept in-memory.
                //var complete = filtered.ToComplete();

            }
        }

    }
}

using GeoAPI.Geometries;
using GeoDatabase;
using Itinero;
using Itinero.IO.Osm;
using Itinero.IO.Shape;
using Itinero.Osm.Vehicles;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using osm_importer;
using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using static MapsOperations.DBLayer;

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

        public OsmStreamSource ImportOSMXML(Stream streamToImport)
        {
            return new XmlOsmStreamSource(streamToImport);
        }
        public OsmStreamSource ImportOSMPBF(Stream streamToImport)
        {
            return new PBFOsmStreamSource(streamToImport);
        }
        public void ImportOSM(string pathToOSM, DBLayer dal, bool forceIfTableEmpty = false, MapBoundingBox mapBoundingBox = null)
        {
            using (var fileStream = File.OpenRead(pathToOSM))
            {
                // create source stream.
                OsmStreamSource source;
                if (Path.GetExtension(pathToOSM).ToLower().Contains("pbf"))
                    source = ImportOSMPBF(fileStream);
                else
                    source = ImportOSMXML(fileStream);

                dal.ImportNodes(source, true, mapBoundingBox);
                dal.ImportWays(source);
                dal.ClearUnboundNodes();
            }
        }
        public void SaveDBToShape(DBLayer dBLayer, string shapeFileName)
        {
            var allNodesCache = dBLayer.GetAllNodesDictionary();
            var nodes = dBLayer.GetBoundNodes();

            SaveNodesToShape(shapeFileName, allNodesCache, nodes);
        }

        public void SaveNodesToShape(string shapeFileName, Dictionary<long, GeoNode> allNodesCache, List<GeoNode> nodesToSave)
        {
            string firstNameAttribute = "a";
            string lastNameAttribute = "b";

            //create geometry factory
            IGeometryFactory geomFactory = NtsGeometryServices.Instance.CreateGeometryFactory();


            IList<Feature> features = new List<Feature>();

            for (int i = 0; i < nodesToSave.Count - 1; i++)
            {
                GeoNode node1 = nodesToSave[i];
                GeoNode node2 = nodesToSave[i + 1];
                //var ngbGeoNode = allNodesCache[ngb.NodeId];
                var line = geomFactory.CreateLineString(new[] { new Coordinate(node1.Longitude, node1.Latitude), new Coordinate(node2.Longitude, node2.Latitude) });
                AttributesTable t1 = new AttributesTable();
                t1.AddAttribute(firstNameAttribute, node1.OSMId);
                t1.AddAttribute(lastNameAttribute, node2.OSMId);

                Feature feat = new Feature(line, t1);
                features.Add(feat);
            }
            var dirName = Path.GetDirectoryName(shapeFileName);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            var writer = new ShapefileDataWriter(shapeFileName)
            {
                Header = ShapefileDataWriter.GetHeader(features[0], features.Count)
            };
            writer.Write(features);
        }
    }
}

using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace osm_importer
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ImportOSM(@"C:\Users\Alex\Downloads\map", @"C:\Users\Alex\Downloads\export.txt");
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
                               select new Point((float)((Node)osmGeo).Latitude, (float)((Node)osmGeo).Longitude);

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

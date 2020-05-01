using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

namespace osm_importer
{
    public static class GeoUtils
    {
        public static double GetDistance(double latFrom, double lonFrom, double latTo, double lonTo)
        {
            var geoCoordFrom = new GeoCoordinate(latFrom, lonFrom);
            var geoCoordTo = new GeoCoordinate(latTo, lonTo);
            return geoCoordFrom.GetDistanceTo(geoCoordTo);
        }
    }
}

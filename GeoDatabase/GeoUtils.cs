using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osm_importer
{
    public static class Haversine
    {
        static double R = 6372.8; // In kilometers
        static double DoubleR = R * 2;
        public static double Calculate(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            lat1 = ToRadians(lat1);
            lat2 = ToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = Math.Asin(Math.Sqrt(a));
            return DoubleR * c;
        }

        public static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }

    public class MapBoundingBox
    {
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLon { get; set; }
    }
}

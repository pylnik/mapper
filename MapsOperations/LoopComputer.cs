using Itinero;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapsOperations
{
    public class LoopComputer
    {
        public float MinLength { get; set; }
        public float MaxLength { get; set; }
        public void Compute(RouterDb routerDb)
        {
            var coord = routerDb.Network.GetVertex(0);
            var router = new Router(routerDb);
            var edgeEnum = routerDb.Network.GeometricGraph.GetEdgeEnumerator(0);
        }


    }
}

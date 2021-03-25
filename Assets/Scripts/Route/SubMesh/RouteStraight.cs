using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteStraight : RouteSubMesh
    {
        public RouteStraight(RoutePoint start, RoutePoint end)
        {
            m_RouteMeshType = RouteSubMeshType.Straight;
        }

    }
}

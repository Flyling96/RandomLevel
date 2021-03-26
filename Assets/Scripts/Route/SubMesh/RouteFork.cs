using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteFork : RouteSubMesh
    {
        public RouteFork(RoutePoint start, RoutePoint center, RoutePoint end, RoutePoint fork)
        {
            m_RouteMeshType = RouteSubMeshType.Fork;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteFork : RouteSubMesh
    {
        public RouteFork(Vector3 start, Vector3 center, Vector3 end, Vector3 fork)
        {
            m_RouteMeshType = RouteSubMeshType.Fork;
        }
    }
}

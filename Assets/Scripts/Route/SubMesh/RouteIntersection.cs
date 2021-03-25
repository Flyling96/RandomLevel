using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteIntersection : RouteSubMesh
    {
        public RouteIntersection(Vector3 start, Vector3 center, Vector3 end, Vector3 fork0, Vector3 fork1)
        {
            m_RouteMeshType = RouteSubMeshType.Intersection;
        }
    }
}

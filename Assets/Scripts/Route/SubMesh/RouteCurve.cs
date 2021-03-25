using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteCurve :RouteSubMesh
    {
        public RouteCurve(Vector3 start, Vector3 turn, Vector3 end)
        {
            m_RouteMeshType = RouteSubMeshType.Curve;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteStraight : RouteSubMesh
    {
        RoutePoint m_Start = null;
        RoutePoint m_End = null;

        public RouteStraight(RoutePoint start, RoutePoint end)
        {
            m_RouteMeshType = RouteSubMeshType.Straight;
            m_Start = start;
            m_End = end;
            CaculateRealRoutePoints();
        }

        public override void CaculateRealRoutePoints()
        {
            base.CaculateRealRoutePoints();
            m_RealRoutePoints.Add(m_Start.m_LocalPos);
            m_RealRoutePoints.Add(m_End.m_LocalPos);
        }

        public override void CaculateMesh()
        {
            base.CaculateMesh();

            var startPos = m_Start.m_LocalPos;
            var endPos = m_End.m_LocalPos;

            CaculateStraightVertex(startPos, endPos);

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                var index0 = m_RouteCirclePointCount + i;
                var index1 = m_RouteCirclePointCount + (i + 1) % m_RouteCirclePointCount;
                var index2 = i;
                var index3 = (i + 1) % m_RouteCirclePointCount;
                m_TriangleList.Add(index0);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index2);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index3);
                m_TriangleList.Add(index2);
            }


        }
    }
}

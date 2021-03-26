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
            var dir = (endPos - startPos).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;
            right = Vector3.Cross(up, dir).normalized;
            up = Vector3.Cross(dir, right).normalized;

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                var rad = 2 * Mathf.PI * i / m_RouteCirclePointCount;
                var point = Mathf.Sin(rad) * m_RouteCircleRadius  * right +
                    Mathf.Cos(rad) * m_RouteCircleRadius  * up;
                var normal = point.normalized;
                point += startPos;
                m_VertexList.Add(point);
                m_NormalList.Add(normal);
            }

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                var rad = 2 * Mathf.PI * i / m_RouteCirclePointCount;
                var point = Mathf.Sin(rad) * m_RouteCircleRadius * right +
                    Mathf.Cos(rad) * m_RouteCircleRadius * up;
                var normal = point.normalized;
                point += endPos;
                m_VertexList.Add(point);
                m_NormalList.Add(normal);
            }

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

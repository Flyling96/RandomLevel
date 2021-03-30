using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteStraight : RouteSubMesh
    {

        public RouteStraight(RoutePoint start, RoutePoint end,float radius,int pointCount):base(radius,pointCount)
        {
            m_RouteMeshType = RouteSubMeshType.Straight;
            m_Start = start;
            m_End = end;
            m_Start.AddSubMesh(this);
            m_End.AddSubMesh(this);
            CaculateRealRoutePoints();
        }

        public override void CaculateRealRoutePoints()
        {
            base.CaculateRealRoutePoints();
            m_RealRoutePoints.Add(m_Start.m_LocalPos);
            m_RealRoutePoints.Add(m_End.m_LocalPos);
            CaculateDistance();
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

        public override (bool, Vector3) SamplePos(float dis)
        {
            if(m_EnterPoint == null || m_NextPoint == null || m_Distance == 0)
            {
                return (false,Vector3.zero);
            }

            return (true, Vector3.Lerp(m_EnterPoint.m_LocalPos, m_NextPoint.m_LocalPos, Mathf.Clamp01(dis / m_Distance)));
        }
    }
}

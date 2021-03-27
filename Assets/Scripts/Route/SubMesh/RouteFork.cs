using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteFork : RouteSubMesh
    {
        RoutePoint m_Start;
        RoutePoint m_End;
        RoutePoint m_Center;
        List<RoutePoint> m_ForkPoints;

        public RouteFork(RoutePoint center)
        {
            m_RouteMeshType = RouteSubMeshType.Fork;
            m_Center = center;
            m_Start = center.m_PrePoint;
            m_End = center.m_ProPoint;
            m_ForkPoints = center.m_ForkPoints;
        }

        public override void CaculateMesh()
        {
            base.CaculateMesh();

            var startPos = m_Start.m_LocalPos;
            var endPos = m_End.m_LocalPos;

            CaculateStraightVertex(startPos, endPos);

            CreateHole(m_ForkPoints[0]);

        }

        public Vector3[] CaculateHolePoints(int centerIndex)
        {
            var startPos = m_Start.m_LocalPos;
            var endPos = m_End.m_LocalPos;

            var forward = (endPos - startPos).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;
            right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            var rad = 2 * Mathf.PI * centerIndex / m_RouteCirclePointCount;
            var holeUp = Mathf.Sin(rad) * m_RouteCircleRadius * right +
                Mathf.Cos(rad) * m_RouteCircleRadius * up ;

            var holeCenter = holeUp + m_Center.m_LocalPos;

            float[] dirValues = new float[m_RouteCirclePointCount / 4 + 1];
            dirValues[0] = holeUp.magnitude;
            holeUp = holeUp.normalized;
            for (int i = 1; i < m_RouteCirclePointCount / 4 + 1; i++)
            {
                rad = 2 * Mathf.PI * (centerIndex + i) / m_RouteCirclePointCount;
                var dir = Mathf.Sin(rad) * m_RouteCircleRadius * right +
                        Mathf.Cos(rad) * m_RouteCircleRadius * up;
                dirValues[i] = Vector3.Dot(dir, holeUp);
            }

            var holeRight = Vector3.Cross(forward, holeUp);

            Vector3[] circlePoints = new Vector3[m_RouteCirclePointCount];

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                rad = 2 * Mathf.PI * i / m_RouteCirclePointCount;
                int dirIndex = Mathf.Min(Mathf.Abs(i % (m_RouteCirclePointCount / 2) - 0), 
                    Mathf.Abs(i % (m_RouteCirclePointCount / 2) - m_RouteCirclePointCount / 2));

                var point = Mathf.Sin(rad) * m_RouteCircleRadius * holeRight +
                        Mathf.Cos(rad) * m_RouteCircleRadius * forward +
                        (dirValues[dirIndex] - dirValues[0]) * holeUp;

                point += holeCenter;
                circlePoints[i] = point;
            }

            return circlePoints;
        }

        public void CreateHole(RoutePoint forkPoint)
        {
            var startPos = m_Start.m_LocalPos;
            var endPos = m_End.m_LocalPos;

            var forward = (endPos - startPos).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;
            right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            var holeDir = (forkPoint.m_LocalPos - m_Center.m_LocalPos).normalized;
            float radOffset = Mathf.Acos(Vector3.Dot(holeDir, up)) +
                Vector3.Dot(Vector3.Cross(up, holeDir), forward) < 0 ? Mathf.PI : 0;

            float centerPro = radOffset / (2 * Mathf.PI);
            int centerIndex = (int)(centerPro * m_RouteCirclePointCount);

            Vector3[] holePoints = CaculateHolePoints(centerIndex);
            if (holePoints == null || holePoints.Length < 1)
            {
                return;
            }

            m_TriangleList.Clear();
            int vertexStart = m_VertexList.Count;
            int preCircleVertexStart = 0;
            int proCircleVertexStart = m_RouteCirclePointCount;

            for (int i = -m_RouteCirclePointCount / 4; i < m_RouteCirclePointCount / 4; i++)
            {
                var index0 = vertexStart + (i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index1 = vertexStart + (i + 1 + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index2 = proCircleVertexStart + (centerIndex + i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index3 = proCircleVertexStart + (centerIndex + i + 1 + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                m_TriangleList.Add(index0);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index2);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index3);
                m_TriangleList.Add(index2);
            }

            for (int i = -m_RouteCirclePointCount / 4; i < m_RouteCirclePointCount / 4; i++)
            {
                var index0 = preCircleVertexStart + (centerIndex + i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index1 = preCircleVertexStart + (centerIndex + i + 1 + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index2 = vertexStart + (m_RouteCirclePointCount / 2 - i) % m_RouteCirclePointCount;
                var index3 = vertexStart + (m_RouteCirclePointCount / 2 - i - 1) % m_RouteCirclePointCount;
                m_TriangleList.Add(index0);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index2);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index3);
                m_TriangleList.Add(index2);
            }

            for (int i = m_RouteCirclePointCount / 4; i < m_RouteCirclePointCount * 3 / 4; i++)
            {
                var index0 = preCircleVertexStart + (centerIndex + i) % m_RouteCirclePointCount;
                var index1 = preCircleVertexStart + (centerIndex + i + 1) % m_RouteCirclePointCount;
                var index2 = proCircleVertexStart + (centerIndex + i) % m_RouteCirclePointCount;
                var index3 = proCircleVertexStart + (centerIndex + i + 1) % m_RouteCirclePointCount;
                m_TriangleList.Add(index0);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index2);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index3);
                m_TriangleList.Add(index2);
            }


            for (int i = 0; i < holePoints.Length; i++)
            {
                m_VertexList.Add(holePoints[i]);
            }

        }


    }
}

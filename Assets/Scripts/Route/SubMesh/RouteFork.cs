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

            m_TriangleList.Clear();
            Dictionary<int, int> preBoldPoints = new Dictionary<int, int>();
            Dictionary<int, int> proBoldPoints = new Dictionary<int, int>();

            for (int j = 0; j < m_ForkPoints.Count; j++)
            {
                int centerIndex = CreateHole(m_ForkPoints[j], preBoldPoints, proBoldPoints);
            }

            var preCircleVertexStart = 0;
            var proCircleVertexStart = m_RouteCirclePointCount;
            HashSet<int> originBoldPoints = new HashSet<int>();
            for(int i =0; i < m_RouteCirclePointCount;i++)
            {
                originBoldPoints.Add(i);
            }
            foreach (var keyValue in preBoldPoints)
            {
                var index0 = preCircleVertexStart + keyValue.Key;
                var index1 = (keyValue.Key + 1) % m_RouteCirclePointCount;
                var boldIndex0 = keyValue.Value;
                if (!preBoldPoints.ContainsKey(index1))
                {
                    continue;
                }
                originBoldPoints.Remove(boldIndex0);
                var boldIndex1 = preBoldPoints[index1];
                index1 = preCircleVertexStart + index1;

                m_TriangleList.Add(index0);
                m_TriangleList.Add(boldIndex0);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(boldIndex0);
                m_TriangleList.Add(boldIndex1);
            }

            foreach (var keyValue in proBoldPoints)
            {
                var index0 = proCircleVertexStart + keyValue.Key;
                var index1 = (keyValue.Key + 1) % m_RouteCirclePointCount;
                var boldIndex0 = keyValue.Value;
                if (!proBoldPoints.ContainsKey(index1))
                {
                    continue;
                }

                var boldIndex1 = proBoldPoints[index1];
                index1 = proCircleVertexStart + index1;

                m_TriangleList.Add(boldIndex0);
                m_TriangleList.Add(index0);
                m_TriangleList.Add(boldIndex1);
                m_TriangleList.Add(boldIndex1);
                m_TriangleList.Add(index0);
                m_TriangleList.Add(index1);
            }

            //foreach (var index in originBoldPoints)
            //{
            //    var index0 = proCircleVertexStart + index;
            //    var index1 = proCircleVertexStart + index + 1;
            //    var index2 = preCircleVertexStart + index;
            //    var index3 = preCircleVertexStart + index + 1;
            //    m_TriangleList.Add(index0);
            //    m_TriangleList.Add(index1);
            //    m_TriangleList.Add(index2);
            //    m_TriangleList.Add(index1);
            //    m_TriangleList.Add(index3);
            //    m_TriangleList.Add(index2);
            //    if (!originBoldPoints.Contains(index - 1))
            //    {
            //        index0 = proCircleVertexStart + index - 1;
            //        index1 = proCircleVertexStart + index;
            //        index2 = preCircleVertexStart + index - 1;
            //        index3 = preCircleVertexStart + index;
            //        m_TriangleList.Add(index0);
            //        m_TriangleList.Add(index1);
            //        m_TriangleList.Add(index2);
            //        m_TriangleList.Add(index1);
            //        m_TriangleList.Add(index3);
            //        m_TriangleList.Add(index2);
            //    }

            //}

        }

        public Vector3[] CaculateHolePoints(int centerIndex)
        {
            var startPos = m_Start.m_LocalPos;
            var endPos = m_End.m_LocalPos;

            var forward = (endPos - startPos).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;
            CaculateCoordinate(forward, out up, out right);

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

            var holeRight = Vector3.Cross(holeUp, forward);

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

        public int CreateHole(RoutePoint forkPoint, Dictionary<int, int> preBoldPoints, Dictionary<int, int> proBoldPoints)
        {
            var startPos = m_Start.m_LocalPos;
            var endPos = m_End.m_LocalPos;

            var forward = (endPos - startPos).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;
            CaculateCoordinate(forward, out up, out right);

            var holeDir = (forkPoint.m_LocalPos - m_Center.m_LocalPos).normalized;
            float radOffset = Vector3.Dot(holeDir, right) > 0 ? Mathf.Acos(Vector3.Dot(holeDir, up)) :
                2 * Mathf.PI - Mathf.Acos(Vector3.Dot(holeDir, up));

            float centerPro = radOffset / (2 * Mathf.PI);
            int centerIndex = Mathf.RoundToInt(centerPro * m_RouteCirclePointCount);

            Vector3[] holePoints = CaculateHolePoints(centerIndex);
            if (holePoints == null || holePoints.Length < 1)
            {
                return -1;
            }

            int vertexStart = m_VertexList.Count;
            int preCircleVertexStart = 0;
            int proCircleVertexStart = m_RouteCirclePointCount;

            for (int i = 0; i < holePoints.Length; i++)
            {
                var normal = (holePoints[i] - m_Center.m_LocalPos).normalized;
                m_VertexList.Add(holePoints[i]);
                m_NormalList.Add(normal);
            }

            for (int i = -m_RouteCirclePointCount / 4; i < m_RouteCirclePointCount / 4 + 1; i++)
            {
                var index0 = vertexStart + (i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index1 = (centerIndex + i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index2 = proCircleVertexStart + index1;

                var point0 = m_VertexList[index0];
                var point2 = m_VertexList[index2];

                if (proBoldPoints.ContainsKey(index1))
                {
                    var dis0 = Vector3.Distance(point0, point2);
                    var point1 = m_VertexList[proBoldPoints[index1]];
                    var dis1 = Vector3.Distance(point1, point2);
                    if(dis0 < dis1)
                    {
                        proBoldPoints[index1] = index0;
                    }
                }
                else
                {
                    proBoldPoints.Add(index1, index0);
                }
            }

            for (int i = -m_RouteCirclePointCount / 4; i < m_RouteCirclePointCount / 4 + 1; i++)
            {
                var index0 = vertexStart + (m_RouteCirclePointCount / 2 - i) % m_RouteCirclePointCount;
                var index1 = (centerIndex + i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index2 = preCircleVertexStart + index1;

                var point0 = m_VertexList[index0];
                var point2 = m_VertexList[index2];

                if (preBoldPoints.ContainsKey(index1))
                {
                    var dis0 = Vector3.Distance(point0, point2);
                    var point1 = m_VertexList[preBoldPoints[index1]];
                    var dis1 = Vector3.Distance(point1, point2);
                    if (dis0 < dis1)
                    {
                        preBoldPoints[index1] = index0;
                    }
                }
                else
                {
                    preBoldPoints.Add(index1, index0);
                }
            }

            //for (int i = -m_RouteCirclePointCount / 4; i < m_RouteCirclePointCount / 4; i++)
            //{
            //    var index0 = vertexStart + (i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
            //    var index1 = vertexStart + (i + 1 + m_RouteCirclePointCount) % m_RouteCirclePointCount;
            //    var index2 = proCircleVertexStart + (centerIndex + i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
            //    var index3 = proCircleVertexStart + (centerIndex + i + 1 + m_RouteCirclePointCount) % m_RouteCirclePointCount;
            //    //m_TriangleList.Add(index0);
            //    //m_TriangleList.Add(index2);
            //    //m_TriangleList.Add(index1);
            //    //m_TriangleList.Add(index1);
            //    //m_TriangleList.Add(index2);
            //    //m_TriangleList.Add(index3);

            //}

            //for (int i = -m_RouteCirclePointCount / 4; i < m_RouteCirclePointCount / 4; i++)
            //{
            //    var index0 = preCircleVertexStart + (centerIndex + i + m_RouteCirclePointCount) % m_RouteCirclePointCount;
            //    var index1 = preCircleVertexStart + (centerIndex + i + 1 + m_RouteCirclePointCount) % m_RouteCirclePointCount;
            //    var index2 = vertexStart + (m_RouteCirclePointCount / 2 - i) % m_RouteCirclePointCount;
            //    var index3 = vertexStart + (m_RouteCirclePointCount / 2 - i - 1) % m_RouteCirclePointCount;
            //    //m_TriangleList.Add(index0);
            //    //m_TriangleList.Add(index2);
            //    //m_TriangleList.Add(index1);
            //    //m_TriangleList.Add(index1);
            //    //m_TriangleList.Add(index2);
            //    //m_TriangleList.Add(index3);
            //}

            var forkForward = (forkPoint.m_LocalPos - m_Center.m_LocalPos).normalized;
            var forkUp = Vector3.up;
            var forkRight = Vector3.right;
            CaculateCoordinate(forkForward, out forkUp, out forkRight);

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                var rad = 2 * Mathf.PI * i / m_RouteCirclePointCount;
                var point = Mathf.Sin(rad) * m_RouteCircleRadius * forkRight +
                    Mathf.Cos(rad) * m_RouteCircleRadius * forkUp;
                var normal = point.normalized;
                point += forkPoint.m_LocalPos;
                m_VertexList.Add(point);
                m_NormalList.Add(normal);
            }

            float angle = Vector3.Angle(forkUp, forward) * Mathf.Deg2Rad
                + (Vector3.Dot(forkRight, forward) < 0 ? Mathf.PI : 0);
            int angleIndex = Mathf.RoundToInt(angle / (2 * Mathf.PI) * m_RouteCirclePointCount);

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                var index0 = vertexStart + m_RouteCirclePointCount + i;
                var index1 = vertexStart + m_RouteCirclePointCount + (i + 1) % m_RouteCirclePointCount;
                var index2 = vertexStart + (angleIndex - i + 2 * m_RouteCirclePointCount) % m_RouteCirclePointCount;
                var index3 = vertexStart + (angleIndex - i - 1 + 2 * m_RouteCirclePointCount) % m_RouteCirclePointCount;
                m_TriangleList.Add(index0);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index2);
                m_TriangleList.Add(index1);
                m_TriangleList.Add(index3);
                m_TriangleList.Add(index2);
            }

            return centerIndex;

        }


    }
}

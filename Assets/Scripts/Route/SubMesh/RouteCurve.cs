using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    public class RouteCurve :RouteSubMesh
    {
        RoutePoint m_Turn = null;

        public RouteCurve(RoutePoint turn, float radius, int pointCount) : base(radius, pointCount)
        {
            m_RouteMeshType = RouteSubMeshType.Curve;
            m_Start = turn.m_PrePoint;
            m_Turn = turn;
            m_End = turn.m_ProPoint;
            m_Start.AddSubMesh(this);
            m_End.AddSubMesh(this);
            m_Turn.AddSubMesh(this);
            CaculateRealRoutePoints();
        }

        public override void CaculateRealRoutePoints()
        {
            base.CaculateRealRoutePoints();
            var dir0 = m_Turn.m_LocalPos - m_Start.m_LocalPos;
            var dir1 = m_End.m_LocalPos - m_Turn.m_LocalPos;
            int pointCount = (int)(dir0.magnitude + dir1.magnitude) * 2;
            for (int j = 0; j < pointCount; j++)
            {
                var point = Bezier2(m_Start.m_LocalPos, m_Turn.m_LocalPos, m_End.m_LocalPos, j * 1.0f / (pointCount - 1));
                m_RealRoutePoints.Add(point);
            }
            CaculateDistance();
        }

        public override void CaculateMesh()
        {
            base.CaculateMesh();
            if (m_RealRoutePoints.Count < 2)
            {
                return;
            }

            var cylinderPoints = CaculateCylinderPoints();

            float disX = 0;
            for (int j = 0; j < m_RealRoutePoints.Count; j++)
            {
                if(j > 0)
                {
                    disX += (m_RealRoutePoints[j] - m_RealRoutePoints[j - 1]).magnitude;
                }
                var point = m_RealRoutePoints[j];
                float disY = 0;
                for (int i = 0; i < m_RouteCirclePointCount; i++)
                {
                    if (i > 0)
                    {
                        if (i < m_RouteCirclePointCount / 2)
                        {
                            disY += (cylinderPoints[j, i] - cylinderPoints[j, i - 1]).magnitude;
                        }
                        else
                        {
                            disY -= (cylinderPoints[j, i] - cylinderPoints[j, i - 1]).magnitude;
                        }
                    }
                    var cylinderPoint = cylinderPoints[j, i];
                    var normal = (cylinderPoint - point).normalized;
                    var uv = new Vector2(disX, disY);
                    m_VertexList.Add(cylinderPoint);
                    m_NormalList.Add(normal);
                    m_UVList.Add(uv);
                }
            }

            for (int j = 1; j < m_RealRoutePoints.Count; j++)
            {
                for (int i = 0; i < m_RouteCirclePointCount; i++)
                {
                    var index0 = (j - 1) * m_RouteCirclePointCount + i;
                    var index1 = (j - 1) * m_RouteCirclePointCount + (i + 1) % m_RouteCirclePointCount;// cylinderPoints[j - 1, (i + 1) % circlePointCount];
                    var index2 = j * m_RouteCirclePointCount + i;
                    var index3 = j * m_RouteCirclePointCount + (i + 1) % m_RouteCirclePointCount;
                    m_TriangleList.Add(index0);
                    m_TriangleList.Add(index1);
                    m_TriangleList.Add(index2);
                    m_TriangleList.Add(index1);
                    m_TriangleList.Add(index3);
                    m_TriangleList.Add(index2);
                }
            }

        }

        Vector3 Bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 p0p1 = (1 - t) * p0 + t * p1;
            Vector3 p1p2 = (1 - t) * p1 + t * p2;
            Vector3 res = (1 - t) * p0p1 + t * p1p2;
            return res;
        }

        Vector2 CalculateMidPointOffset2D(Vector2 a, Vector2 b)
        {
            a = a.normalized;
            b = b.normalized;
            Vector2 result;

            if (a == b)
            {
                result = new Vector2(-a.y, a.x);
                return result;
            }

            if (a.x == -b.y && a.y == b.x)
            {
                result = a - b;
                return result;
            }

            float x = (b.x - a.x) / (a.x * b.y - b.x * a.y);
            float y = (b.y - a.y) / (a.x * b.y - b.x * a.y);
            result = new Vector2(x, y);
            return result;
        }

        (Vector3, float) CalculatePlanePerpendicular(Vector3 dir0, Vector3 dir1, Vector3 point)
        {
            var cross = Vector3.Cross(dir0, dir1);
            Quaternion rot = Quaternion.FromToRotation(cross, Vector3.up);
            Matrix4x4 trs = Matrix4x4.TRS(point, rot, Vector3.one);
            dir0 = trs.MultiplyVector(dir0);
            dir1 = trs.MultiplyVector(dir1);
            Vector2 dir2D0 = new Vector2(dir0.x, dir0.z);
            Vector2 dir2D1 = new Vector2(dir1.x, dir1.z);
            Vector2 offsetDir = CalculateMidPointOffset2D(dir2D0, dir2D1);
            Vector3 targetDir = new Vector3(-offsetDir.y, 0, offsetDir.x);
            targetDir = Matrix4x4.Inverse(trs).MultiplyVector(targetDir);
            return (targetDir, offsetDir.magnitude);
        }

        Vector3[,] CaculateCylinderPoints()
        {
            List<(Vector3, Vector3, Vector3, float)> pointCircleInfoList = new List<(Vector3, Vector3, Vector3, float)>();
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;

            for (int i = 0; i < m_RealRoutePoints.Count; i++)
            {
                Vector3 pos0, pos1, dir;
                float radiusSize = 1;
                if (i == 0)
                {
                    pos0 = m_Start.m_LocalPos;
                    pos1 = m_Turn.m_LocalPos;
                    dir = -(pos1 - pos0).normalized;
                }
                else if (i == m_RealRoutePoints.Count - 1)
                {
                    pos0 = m_End.m_LocalPos;
                    pos1 = m_Turn.m_LocalPos;
                    dir = -(pos0 - pos1).normalized;
                }
                else
                {
                    pos0 = m_RealRoutePoints[i];
                    pos1 = m_RealRoutePoints[i + 1];
                    var pos2 = m_RealRoutePoints[i - 1];
                    var dir0 = (pos0 - pos2).normalized;
                    var dir1 = (pos1 - pos0).normalized;
                    var keyValue = CalculatePlanePerpendicular(dir0, dir1, pos0);
                    dir = keyValue.Item1;
                    radiusSize = Mathf.Clamp(keyValue.Item2, 1, 3);
                }

                CaculateCoordinate(dir, out up, out right);
                pointCircleInfoList.Add((pos0, up, right, radiusSize));


            }

            Vector3[,] cylinderPoints = new Vector3[m_RealRoutePoints.Count, m_RouteCirclePointCount];
            for (int j = 0; j < pointCircleInfoList.Count; j++)
            {
                var pos = pointCircleInfoList[j].Item1;
                up = pointCircleInfoList[j].Item2;
                right = pointCircleInfoList[j].Item3;
                var radiusSize = pointCircleInfoList[j].Item4;
                for (int i = 0; i < m_RouteCirclePointCount; i++)
                {
                    var rad = 2 * Mathf.PI * i / m_RouteCirclePointCount;
                    var point = Mathf.Sin(rad) * m_RouteCircleRadius * radiusSize * right +
                        Mathf.Cos(rad) * m_RouteCircleRadius * radiusSize * up;
                    point += pos;
                    cylinderPoints[j, i] = point;
                }
            }

            return cylinderPoints;
        }

        public override (bool, Vector3) SamplePos(float dis)
        {
            if (m_EnterPoint == null || m_NextPoint == null || m_Distance == 0)
            {
                return (false, Vector3.zero);
            }

            if (dis > m_Distance)
            {
                return (true, m_NextPoint.m_LocalPos);
            }

            if (m_EnterPoint.m_LocalPos == m_RealRoutePoints[0])
            {
                int index = 0;
                for (int i = 0; i < m_RealPointDises.Length - 1; i++)
                {
                    if (m_RealPointDises[i] < dis && m_RealPointDises[i + 1] >= dis)
                    {
                        index = i;
                        break;
                    }
                }

                var allDis = m_RealPointDises[index + 1] - m_RealPointDises[index];
                dis -= m_RealPointDises[index];

                return (true, Vector3.Lerp(m_RealRoutePoints[index], m_RealRoutePoints[index + 1], Mathf.Clamp01(dis / allDis)));
            }
            else
            {
                int index = 0;
                for (int i = m_RealPointDises.Length - 1; i > 0; i--)
                {
                    var dis0 = m_Distance - m_RealPointDises[i];
                    var dis1 = m_Distance - m_RealPointDises[i - 1];
                    if (dis0 < dis && dis1 >= dis)
                    {
                        index = i;
                        break;
                    }
                }

                var allDis = m_RealPointDises[index] - m_RealPointDises[index - 1];
                dis -= m_Distance - m_RealPointDises[index];

                return (true, Vector3.Lerp(m_RealRoutePoints[index], m_RealRoutePoints[index - 1], Mathf.Clamp01(dis / allDis)));
            }
        }

    }
}

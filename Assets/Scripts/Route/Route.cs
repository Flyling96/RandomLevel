using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    [System.Serializable]
    public class Route : ISerializationCallbackReceiver
    {
        public Vector3 m_Position;

        public Quaternion m_Rotation = Quaternion.identity;

        private Matrix4x4 m_TRS;

        public void UpdateTransform(Vector3 pos,Quaternion rot)
        {
            bool isChange = false;
            if(m_Position != pos)
            {
                m_Position = pos;
                isChange = true;
            }

            if(m_Rotation != rot)
            {
                m_Rotation = rot;
                isChange = true;
            }

            if(isChange)
            {
                m_TRS = Matrix4x4.TRS(m_Position, m_Rotation, Vector3.one);
            }
        }

        public List<RoutePoint> m_Points = new List<RoutePoint>();


        public Route(Vector3 pos,Quaternion rot)
        {
            m_Position = pos;
            m_Rotation = rot;
            m_RealRoutePoints = new List<Vector3>();
        }

        public void AddPoints(Vector3 pos)
        {
            RoutePoint newPoint = new RoutePoint();
            newPoint.m_LocalPos = Matrix4x4.Inverse(m_TRS).MultiplyPoint(pos);
            m_Points.Add(newPoint);
        }

        public void DeletePoints(RoutePoint target)
        {
            if(!m_Points.Contains(target))
            {
                return;
            }

            target.OnDelete();
            m_Points.Remove(target);
        }

        public void Connect(RoutePoint p0,RoutePoint p1)
        {
            p0.OnConnect(p1);
            p1.OnConnect(p0);
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            m_TRS = Matrix4x4.TRS(m_Position, m_Rotation, Vector3.zero);
            if (m_RealRoutePoints == null)
            {
                m_RealRoutePoints = new List<Vector3>();
            }
        }

        List<Vector3> m_RealRoutePoints = new List<Vector3>();

        Vector3 Bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 p0p1 = (1 - t) * p0 + t * p1;
            Vector3 p1p2 = (1 - t) * p1 + t * p2;
            Vector3 res = (1 - t) * p0p1 + t * p1p2;
            return res;
        }

        public void Bezier2Point()
        {
            m_RealRoutePoints.Clear();
            float minDis = 1;
            float maxDis = 3;
            List<Vector3> routePoint = new List<Vector3>();
            routePoint.Add(m_Points[0].m_LocalPos);
            for (int i = 1; i < m_Points.Count - 1; i++)
            {
                var p0 = m_Points[i - 1].m_LocalPos;
                var p1 = m_Points[i].m_LocalPos;
                var p2 = m_Points[i + 1].m_LocalPos;
                var dir0 = p1 - p0;
                var dir1 = p2 - p1;
                if (Vector3.Dot(dir0, dir1) == dir0.magnitude * dir1.magnitude)
                {
                    routePoint.Add(p1);
                }
                else
                {
                    float dis0 = Mathf.Clamp(dir0.magnitude / 3, minDis, maxDis);
                    float dis1 = Mathf.Clamp(dir1.magnitude / 3, minDis, maxDis);
                    routePoint.Add(p1 - dir0.normalized * dis0);
                    routePoint.Add(p1);
                    routePoint.Add(p1 + dir1.normalized * dis1);
                }
            }
            routePoint.Add(m_Points[m_Points.Count - 1].m_LocalPos);

            m_RealRoutePoints.Add(routePoint[0]);
            for(int i =1;i < routePoint.Count - 1;i++)
            {
                var p0 = routePoint[i - 1];
                var p1 = routePoint[i];
                var p2 = routePoint[i + 1];
                var dir0 = p1 - p0;
                var dir1 = p2 - p1;
                if(Mathf.Abs(Vector3.Dot(dir0,dir1) - dir0.magnitude * dir1.magnitude) < 0.01f)
                {
                    m_RealRoutePoints.Add(p1);
                }
                else
                {
                    float pointCount = (dir0.magnitude + dir1.magnitude) * 2;
                    for(int j =1;j< pointCount - 1;j++)
                    {
                        var point = Bezier2(p0, p1, p2, j / pointCount);
                        m_RealRoutePoints.Add(point);
                    }
                }
            }
            m_RealRoutePoints.Add(m_Points[m_Points.Count - 1].m_LocalPos);
        }

        Vector2 CalculateMidPointOffset2D(Vector2 a, Vector2 b)
        {
            a = a.normalized;
            b = b.normalized;
            Vector2 result;

            if (a == b)
            {
                result = new Vector2(-a.y, a.x) ;
                return result;
            }

            if (a.x == -b.y && a.y == b.x)
            {
                result = a  - b ;
                return result;
            }

            float x = (b.x - a.x) / (a.x * b.y - b.x * a.y) ;
            float y = (b.y - a.y) / (a.x * b.y - b.x * a.y) ;
            result = new Vector2(x, y);
            return result;
        }

        (Vector3,float) CalculatePlanePerpendicular(Vector3 dir0,Vector3 dir1,Vector3 point)
        {
            var cross = Vector3.Cross(dir0, dir1);
            Quaternion rot = Quaternion.FromToRotation(cross, Vector3.up);
            Matrix4x4 trs = Matrix4x4.TRS(point, rot, Vector3.one);
            dir0 = trs.MultiplyVector(dir0);
            dir1 = trs.MultiplyVector(dir1);
            Vector2 dir2D0 = new Vector2(dir0.x, dir0.z);
            Vector2 dir2D1 = new Vector2(dir1.x, dir1.z);
            Vector2 offsetDir = CalculateMidPointOffset2D(dir2D0, dir2D1);
            Vector3 targetDir = new Vector3(-offsetDir.y, 0 ,offsetDir.x);
            targetDir = Matrix4x4.Inverse(trs).MultiplyVector(targetDir);
            return (targetDir, offsetDir.magnitude);
        }

        public float radius = 3;
        public int circlePointCount = 20;
        List<int> cylinderTriangleStartList = new List<int>();

        Vector3[,] CaculateCylinderPoints()
        {
            List<(Vector3,Vector3,Vector3,float)> pointCircleInfoList = new List<(Vector3, Vector3, Vector3, float)>();

            //Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, new Vector3(1, 1, 0));
            //Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, Vector3.up);
            Vector3 up =  Vector3.up ;
            Vector3 right =  Vector3.right;

            for (int i = 0; i < m_RealRoutePoints.Count; i++)
            {
                Vector3 pos0, pos1,dir;
                float radiusSize = 1;
                if (i == 0)
                {
                    pos0 = m_RealRoutePoints[i];
                    pos1 = m_RealRoutePoints[i + 1];
                    dir = -(pos1 - pos0).normalized;
                }
                else if( i == m_RealRoutePoints.Count - 1)
                {
                    pos0 = m_RealRoutePoints[i];
                    pos1 = m_RealRoutePoints[i - 1];
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
                    radiusSize = Mathf.Clamp(keyValue.Item2,1,3);
                }
                //Quaternion rot;
                ////当dir与Vector3.up 完全反向时 Quaternion.FromToRotation 会得到 (1,0,0,0);
                //if (dir == Vector3.down)
                //{
                //    rot = Quaternion.AngleAxis(180,Vector3.up);
                //}
                //else
                //{
                //    rot = Quaternion.FromToRotation(Vector3.up, dir);
                //}

                //rot = Quaternion.FromToRotation(Vector3.up, dir);
                //rot = Quaternion.FromToRotation(up, dir);
                //Matrix4x4 pointTRS = Matrix4x4.TRS(pos0, rot, Vector3.one);

                right = Vector3.Cross(up, dir).normalized;
                up = Vector3.Cross(dir, right).normalized;
                pointCircleInfoList.Add((pos0,up,right, radiusSize));


            }

            Vector3[,] cylinderPoints = new Vector3[m_RealRoutePoints.Count, circlePointCount];
            for (int j = 0; j < pointCircleInfoList.Count; j++)
            {
                var pos = pointCircleInfoList[j].Item1;
                up = pointCircleInfoList[j].Item2;
                right = pointCircleInfoList[j].Item3;
                var radiusSize = pointCircleInfoList[j].Item4;
                for (int i = 0; i < circlePointCount; i++)
                {
                    var rad = 2 * Mathf.PI * i / circlePointCount;
                    var point = new Vector3(Mathf.Sin(rad) * radius * radiusSize, 0, Mathf.Cos(rad) * radius * radiusSize);

                    point = Mathf.Sin(rad) * radius * radiusSize * right +
                        Mathf.Cos(rad) * radius * radiusSize * up;
                    point += pos;
                    cylinderPoints[j, i] = point;
                }
            }

            return cylinderPoints;
        }

        [SerializeField,HideInInspector]
        List<Vector3> vertexList = new List<Vector3>();
        [SerializeField,HideInInspector]
        List<int> triangleList = new List<int>();

        public Mesh ConvertToMesh()
        {
            Bezier2Point();
            if (m_RealRoutePoints.Count < 2)
            {
                return null;
            }

            var cylinderPoints = CaculateCylinderPoints();

            List<Vector3> normalList = new List<Vector3>();
            vertexList.Clear();
            triangleList.Clear();
            for (int j = 0; j < m_RealRoutePoints.Count; j++)
            {
                var point = m_RealRoutePoints[j];
                for (int i = 0; i < circlePointCount;i++)
                {
                    var cylinderPoint = cylinderPoints[j, i];
                    var normal = (cylinderPoint - point).normalized;
                    vertexList.Add(cylinderPoint);
                    normalList.Add(normal);
                }
            }

            cylinderTriangleStartList.Clear();
            cylinderTriangleStartList.Add(0);
            for (int j = 1; j < m_RealRoutePoints.Count;j++)
            {
                cylinderTriangleStartList.Add(j * 6 * circlePointCount);
                for (int i =0; i < circlePointCount;i++)
                {
                    var index0 = (j - 1) * circlePointCount + i;
                    var index1 = (j - 1) * circlePointCount + (i + 1) % circlePointCount;// cylinderPoints[j - 1, (i + 1) % circlePointCount];
                    var index2 = j * circlePointCount + i;
                    var index3 = j * circlePointCount + (i + 1) % circlePointCount;
                    triangleList.Add(index0);
                    triangleList.Add(index1);
                    triangleList.Add(index2);
                    triangleList.Add(index1);
                    triangleList.Add(index3);
                    triangleList.Add(index2);
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertexList.ToArray();
            mesh.normals = normalList.ToArray();
            mesh.triangles = triangleList.ToArray();
            return mesh;
        }

        #region Hole
        public int m_HolePointIndex = 5;
        public float m_HolePointPro = 0.5f;
        public float m_HoleDir = Mathf.PI / 2;

        public Vector3[] CaculateHolePoints()
        {
            if(m_RealRoutePoints.Count <= m_HolePointIndex + 1)
            {
                return new Vector3[0];
            }

            Vector3[] res = new Vector3[circlePointCount + 1];

            var p0 = m_RealRoutePoints[m_HolePointIndex];
            var p1 = m_RealRoutePoints[m_HolePointIndex + 1];
            var dir = p1 - p0;

            float centerPro = m_HoleDir / (2 * Mathf.PI);
            int centerIndex = (int)(centerPro * circlePointCount);
            float centerCirclePointPro = centerPro * circlePointCount - centerIndex;

            int index = m_HolePointIndex * circlePointCount;
            var p2 = vertexList[index + centerIndex];
            var p3 = vertexList[index + (centerIndex + 1) % circlePointCount];
            index = (m_HolePointIndex + 1) * circlePointCount;
            var p4 = vertexList[index + centerIndex];
            var p5 = vertexList[index + (centerIndex + 1) % circlePointCount];

            var centerPos = Vector3.Lerp(Vector3.Lerp(p2, p3, centerCirclePointPro), Vector3.Lerp(p4, p5, centerCirclePointPro), m_HolePointPro);

            Debug.Log(centerPos);

            return null;




        }
        #endregion


    }

}

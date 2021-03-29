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

        List<RoutePoint> m_Points = new List<RoutePoint>();


        public Route(Vector3 pos,Quaternion rot)
        {
            m_Position = pos;
            m_Rotation = rot;
            m_RealRoutePoints = new List<Vector3>();
        }

        public void ClearPoints()
        {
            m_Points.Clear();
        }

        public void AddPoints(Vector3 pos)
        {
            RoutePoint newPoint = new RoutePoint(Matrix4x4.Inverse(m_TRS).MultiplyPoint(pos));
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

        public void InsertPoint(RoutePoint start, RoutePoint end, RoutePoint point)
        {
            DisconnectPoint(start, end);
            ConnectPoint(start, point);
            ConnectPoint(point, end);
        }

        void DisconnectPoint(RoutePoint p0, RoutePoint p1)
        {
            p0.OnDisconnect(p1);
            p1.OnDisconnect(p0);
        }

        public void ConnectPoint(int startIndex,int endIndex)
        {
            var start = m_Points[startIndex];
            var end = m_Points[endIndex];
            ConnectPoint(start, end);
        }

        public void ConnectPoint(RoutePoint start, RoutePoint end)
        {
            start.OnConnect(end,false);
            end.OnConnect(start,true);
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            m_TRS = Matrix4x4.TRS(m_Position, m_Rotation, Vector3.one);
            if (m_RealRoutePoints == null)
            {
                m_RealRoutePoints = new List<Vector3>();
            }

            if(m_ComplateRoutePoints == null)
            {
                m_ComplateRoutePoints = new HashSet<RoutePoint>();
            }
        }

        List<Vector3> m_RealRoutePoints = new List<Vector3>();

        HashSet<RoutePoint> m_ComplateRoutePoints = new HashSet<RoutePoint>();
        public void AutoFillPoint()
        {
            m_ComplateRoutePoints.Clear();
            for (int i = 0; i < m_Points.Count;i++)
            {
                var point = m_Points[i];
                m_ComplateRoutePoints.Add(point);
                
                if(point.IsTurn || point.IsFork)
                {
                    var pre = point.m_PrePoint;
                    var pro = point.m_ProPoint;
                    var dir0 = pre.m_LocalPos - point.m_LocalPos;
                    var dir1 = pro.m_LocalPos - point.m_LocalPos;
                    if(dir0.magnitude > radius)
                    {
                        var pos = point.m_LocalPos + dir0.normalized * radius;
                        var p = new RoutePoint(pos);
                        InsertPoint(pre, point, p);
                        m_ComplateRoutePoints.Add(p);
                    }
                    if(dir1.magnitude > radius)
                    {
                        var pos = point.m_LocalPos + dir1.normalized * radius;
                        var p = new RoutePoint(pos);
                        InsertPoint(point, pro, p);
                        m_ComplateRoutePoints.Add(p);
                    }
                }

                if(point.IsFork)
                {
                    for(int j = point.m_ForkPoints.Count - 1; j >-1;j--)
                    {
                        var forkPoint = point.m_ForkPoints[j];
                        var dir = forkPoint.m_LocalPos - point.m_LocalPos;
                        if (dir.magnitude >  radius)
                        {
                            var pos = point.m_LocalPos + dir.normalized * radius;
                            var p = new RoutePoint(pos);
                            InsertPoint(point, forkPoint, p);
                            m_ComplateRoutePoints.Add(p);
                        }
                    }
                }
            }
        }

        List<RouteSubMesh> m_SubMeshList = new List<RouteSubMesh>();
        public void CaculateSubMesh()
        {
            AutoFillPoint();

            m_SubMeshList.Clear();
            foreach(var point in m_ComplateRoutePoints)
            {
                if(point.IsStraight)
                {
                    var straight = CaculateRouteStraight(point);
                    if(straight != null)
                    {
                        m_SubMeshList.Add(straight);
                    }
                }
                else if(point.IsTurn)
                {
                    var curve = CaculateRouteCurve(point);
                    if(curve != null)
                    {
                        m_SubMeshList.Add(curve);
                    }
                }
                else if(point.IsFork)
                {
                    var fork = CaculateRouteFork(point);
                    if(fork != null)
                    {
                        m_SubMeshList.Add(fork);
                    }
                }

            }
        }

        public Mesh ConvertSubMeshes()
        {
            CaculateSubMesh();

            List<Vector3> vertexList = new List<Vector3>();
            List<Vector3> normalList = new List<Vector3>();
            List<int> triangleList = new List<int>();

            for(int i =0;i < m_SubMeshList.Count;i++)
            {
                var subMesh = m_SubMeshList[i];
                subMesh.CaculateMesh();
                var meshData = subMesh.ConvertMesh(vertexList.Count);
                vertexList.AddRange(meshData.m_VertexList);
                normalList.AddRange(meshData.m_NormalList);
                triangleList.AddRange(meshData.m_TriangleList);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertexList.ToArray();
            mesh.normals = normalList.ToArray();
            mesh.triangles = triangleList.ToArray();
            return mesh;
        }

        RouteStraight CaculateRouteStraight(RoutePoint point)
        {
            var pre = point.m_PrePoint;
            var pro = point.m_ProPoint;
            //中间点不做处理
            if(pre!= null && pre.IsStraight)
            {
                return null;
            }
            if(pro == null)
            {
                return null;
            }

            var start = point;
            var end = pro;

            while(pro!=null && pro.IsStraight)
            {
                end = pro;
                pro = pro.m_ProPoint;
            }

            RouteStraight straight = new RouteStraight(start, end);
            return straight;
        }

        RouteCurve CaculateRouteCurve(RoutePoint point)
        {
            var pre = point.m_PrePoint;
            var pro = point.m_ProPoint;
            if(!pre.IsStraight || !pro.IsStraight)
            {
                return null;
            }

            RouteCurve curve = new RouteCurve(point);
            return curve;
        }

        RouteFork CaculateRouteFork(RoutePoint point)
        {
            var pre = point.m_PrePoint;
            var pro = point.m_ProPoint;
            if (!pre.IsStraight || !pro.IsStraight)
            {
                return null;
            }
            RouteFork fork = new RouteFork(point);
            return fork;
        }


        #region ConvertMesh
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

        Vector3[,] CaculateCylinderPoints()
        {
            List<(Vector3,Vector3,Vector3,float)> pointCircleInfoList = new List<(Vector3, Vector3, Vector3, float)>();
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
                    var point = Mathf.Sin(rad) * radius * radiusSize * right +
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
        Dictionary<int,List<int>> triangleDic = new Dictionary<int, List<int>>();

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

            if(triangleDic == null)
            {
                triangleDic = new Dictionary<int, List<int>>();
            }

            triangleDic.Clear();
            for (int j = 1; j < m_RealRoutePoints.Count;j++)
            {
                List<int> triangleList = new List<int>();
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
                triangleDic.Add(j - 1, triangleList);
            }

            var mesh = new Mesh();
            mesh.vertices = vertexList.ToArray();
            mesh.normals = normalList.ToArray();
            List<int> triangles = new List<int>();
            foreach(var list in triangleDic.Values)
            {
                triangles.AddRange(list);
            }

            mesh.triangles = triangles.ToArray();
            return mesh;
        }
        #endregion

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

            float centerPro = m_HoleDir / (2 * Mathf.PI);
            int centerIndex = (int)(centerPro * circlePointCount);

            var p2 = vertexList[m_HolePointIndex * circlePointCount + centerIndex];
            var p3 = vertexList[(m_HolePointIndex + 1) * circlePointCount + centerIndex];

            var centerPos = Vector3.Lerp(p2,p3, m_HolePointPro);
            var up = p2 - p0;

            float[] dirValues = new float[circlePointCount / 4 + 1];
            dirValues[0] = up.magnitude;
            up = up.normalized;
            for (int i = 1; i < circlePointCount / 4 + 1; i++)
            {
                var point = vertexList[m_HolePointIndex * circlePointCount + (centerIndex + i)% circlePointCount];
                dirValues[i] = Vector3.Dot(point - p0, up);
            }

            var forward = (p1 - p0).normalized;
            var right = Vector3.Cross(forward, up);

            Vector3[] circlePoints = new Vector3[circlePointCount];

            for(int i = 0; i < circlePointCount; i++)
            {
                var rad = 2 * Mathf.PI * i / circlePointCount;
                int dirIndex = Mathf.Min(Mathf.Abs(i % (circlePointCount / 2) - 0), Mathf.Abs(i % (circlePointCount / 2) - circlePointCount / 2));
                var point = Mathf.Sin(rad) * radius  * right +
                        Mathf.Cos(rad) * radius  * forward + 
                        (dirValues[dirIndex] - dirValues[0]) * up;

                point += centerPos;
                circlePoints[i] = point;
            }

            Debug.Log(centerPos);
     
            return circlePoints;
        }

        public Mesh CreateHole()
        {
            Vector3[] holePoints = CaculateHolePoints();
            if(holePoints == null || holePoints.Length < 1)
            {
                return null;
            }

            List<int> triangleList = new List<int>();
            float centerPro = m_HoleDir / (2 * Mathf.PI);
            int centerIndex = (int)(centerPro * circlePointCount);
            int vertexStart = vertexList.Count;
            int preCircleVertexStart = m_HolePointIndex * circlePointCount;
            int proCircleVertexStart = preCircleVertexStart + circlePointCount;

            int j = m_HolePointIndex + 1;
            for (int i = - circlePointCount /4 ; i < circlePointCount / 4 ; i++)
            {
                var index0 = vertexStart + (i + circlePointCount) % circlePointCount;
                var index1 = vertexStart + (i + 1 + circlePointCount) % circlePointCount;
                var index2 = proCircleVertexStart + (centerIndex + i + circlePointCount)% circlePointCount;
                var index3 = proCircleVertexStart + (centerIndex + i + 1 + circlePointCount) % circlePointCount;
                triangleList.Add(index0);
                triangleList.Add(index1);
                triangleList.Add(index2);
                triangleList.Add(index1);
                triangleList.Add(index3);
                triangleList.Add(index2);
            }

            for (int i = -circlePointCount / 4; i < circlePointCount / 4; i++)
            {
                var index0 = preCircleVertexStart + (centerIndex + i + circlePointCount) % circlePointCount;
                var index1 = preCircleVertexStart + (centerIndex + i + 1 + circlePointCount) % circlePointCount;
                var index2 = vertexStart + (circlePointCount / 2 - i) % circlePointCount;
                var index3 = vertexStart + (circlePointCount / 2 - i - 1) % circlePointCount;
                triangleList.Add(index0);
                triangleList.Add(index1);
                triangleList.Add(index2);
                triangleList.Add(index1);
                triangleList.Add(index3);
                triangleList.Add(index2);
            }

            for (int i = circlePointCount / 4; i < circlePointCount * 3 / 4; i++)
            {
                var index0 = preCircleVertexStart + (centerIndex + i ) % circlePointCount;
                var index1 = preCircleVertexStart + (centerIndex + i + 1 ) % circlePointCount;
                var index2 = proCircleVertexStart + (centerIndex + i ) % circlePointCount;
                var index3 = proCircleVertexStart + (centerIndex + i + 1 ) % circlePointCount;
                triangleList.Add(index0);
                triangleList.Add(index1);
                triangleList.Add(index2);
                triangleList.Add(index1);
                triangleList.Add(index3);
                triangleList.Add(index2);
            }


            for (int i =0;i < holePoints.Length;i++)
            {
                vertexList.Add(holePoints[i]);
            }

            triangleDic[m_HolePointIndex] = triangleList;

            var mesh = new Mesh();
            mesh.vertices = vertexList.ToArray();
            //mesh.normals = normalList.ToArray();
            List<int> triangles = new List<int>();
            foreach (var list in triangleDic.Values)
            {
                triangles.AddRange(list);
            }

            mesh.triangles = triangles.ToArray();
            return mesh;


        }
        #endregion


    }

}

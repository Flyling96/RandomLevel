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
            if (m_ComplateRoutePoints == null)
            {
                m_ComplateRoutePoints = new HashSet<RoutePoint>();
            }

            if (m_GateRoutePoints == null)
            {
                m_GateRoutePoints = new List<RoutePoint>();
            }

            if (m_SubMeshList == null)
            {
                m_SubMeshList = new List<RouteSubMesh>();
            }

            if(m_Points == null)
            {
                m_Points = new List<RoutePoint>();
            }
        }

        HashSet<RoutePoint> m_ComplateRoutePoints = new HashSet<RoutePoint>();
        List<RoutePoint> m_GateRoutePoints = new List<RoutePoint>();

        public float m_CircleRadius = 3;
        public int m_CirclePointCount = 24;

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
                    if(dir0.magnitude > m_CircleRadius)
                    {
                        var pos = point.m_LocalPos + dir0.normalized * m_CircleRadius;
                        var p = new RoutePoint(pos);
                        InsertPoint(pre, point, p);
                        m_ComplateRoutePoints.Add(p);
                    }
                    if(dir1.magnitude > m_CircleRadius)
                    {
                        var pos = point.m_LocalPos + dir1.normalized * m_CircleRadius;
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
                        if (dir.magnitude > m_CircleRadius)
                        {
                            var pos = point.m_LocalPos + dir.normalized * m_CircleRadius;
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
            m_GateRoutePoints.Clear();
            foreach (var point in m_ComplateRoutePoints)
            {
                if (point.IsStraight)
                {
                    var straight = CaculateRouteStraight(point);
                    if (straight != null)
                    {
                        m_SubMeshList.Add(straight);
                    }

                    if (point.IsGate)
                    {
                        if (!m_GateRoutePoints.Contains(point))
                        {
                            m_GateRoutePoints.Add(point);
                        }
                    }
                }
                else if (point.IsTurn)
                {
                    var curve = CaculateRouteCurve(point);
                    if (curve != null)
                    {
                        m_SubMeshList.Add(curve);
                    }
                }
                else if (point.IsFork)
                {
                    var fork = CaculateRouteFork(point);
                    if (fork != null)
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
            List<Vector2> uvList = new List<Vector2>();
            List<int> triangleList = new List<int>();

            for(int i =0;i < m_SubMeshList.Count;i++)
            {
                var subMesh = m_SubMeshList[i];
                subMesh.CaculateMesh();
                var meshData = subMesh.ConvertMesh(vertexList.Count);
                vertexList.AddRange(meshData.m_VertexList);
                normalList.AddRange(meshData.m_NormalList);
                uvList.AddRange(meshData.m_UVList);
                triangleList.AddRange(meshData.m_TriangleList);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertexList.ToArray();
            mesh.normals = normalList.ToArray();
            mesh.uv = uvList.ToArray();
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

            RouteStraight straight = new RouteStraight(start, end, m_CircleRadius,m_CirclePointCount);
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

            RouteCurve curve = new RouteCurve(point, m_CircleRadius, m_CirclePointCount);
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
            RouteFork fork = new RouteFork(point, m_CircleRadius, m_CirclePointCount);
            return fork;
        }

        #region Sample
        private RouteSubMesh m_CurrentSubMesh;
        private float m_HasMoveDis = 0;
        private float m_SampleTime = 0;
        public float m_MoveSpeed = 1.0f;

        private Vector2 m_InputDir = Vector2.zero;
        public float m_InputDelayTime = 3;
        private float m_InputTime = 0;
        public void UpdateInputDir(Vector2 inputDir)
        {
            if(inputDir == Vector2.zero)
            {
                return;
            }

            if(m_InputDir != inputDir)
            {
                m_InputDir = inputDir;
                m_InputTime = m_InputDelayTime;
            }
        }

        public void StartRoute(int gateIndex)
        {
            if(m_GateRoutePoints.Count < gateIndex + 1)
            {
                return;
            }

            var point = m_GateRoutePoints[gateIndex];
            var subMesh = point.m_BelongSubMeshes[0];
            if(subMesh == null)
            {
                return;
            }

            m_CurrentSubMesh = subMesh;
            m_CurrentSubMesh.EnterSubMesh(point, Vector2.zero);
            m_SampleTime = 0;
            m_HasMoveDis = 0;

        }

        public (bool,Vector3) Sample(float deltaTime)
        {
            if(m_InputTime > 0)
            {
                m_InputTime -= deltaTime;
                if(m_InputTime <= 0)
                {
                    m_InputDir = Vector2.zero;
                }
            }

            if(m_CurrentSubMesh == null)
            {
                return (false, Vector3.zero);
            }
            m_SampleTime += deltaTime;

            float dis = m_MoveSpeed * m_SampleTime;
            if (m_CurrentSubMesh.m_Distance + m_HasMoveDis < dis) 
            {
                var nextPoint = m_CurrentSubMesh.m_NextPoint;
                if(nextPoint != null)
                {
                    m_HasMoveDis += m_CurrentSubMesh.m_Distance;
                    m_CurrentSubMesh = nextPoint.GetOtherSubMesh(m_CurrentSubMesh);
                    if(m_CurrentSubMesh == null)
                    {
                        return (true, m_TRS.MultiplyPoint(nextPoint.m_LocalPos));
                    }
                    else
                    {
                        m_CurrentSubMesh.EnterSubMesh(nextPoint, m_InputDir);
                    }
                }
            }

            dis -= m_HasMoveDis;
            var res = m_CurrentSubMesh.SamplePos(dis);
            res.Item2 = m_TRS.MultiplyPoint(res.Item2);
            return res;

        }

        #endregion
    }

}

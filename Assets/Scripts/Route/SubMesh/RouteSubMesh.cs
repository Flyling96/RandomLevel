using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DragonSlay.Route
{
    public struct MeshData
    {
        public List<Vector3> m_VertexList;
        public List<Vector3> m_NormalList;
        public List<Vector2> m_UVList;
        public List<int> m_TriangleList;
    }

    [Serializable]
    public class RouteSubMesh
    {
        public enum RouteSubMeshType
        {
            Straight,       //直线
            Curve,          //弯道
            Fork,           //三岔路
            Intersection,   //十字路口
        }

        public int m_RouteCirclePointCount = 48;
        public float m_RouteCircleRadius = 3;
        public float m_Distance = 0;

        protected RoutePoint m_Start = null;
        protected RoutePoint m_End = null;

        protected RouteSubMeshType m_RouteMeshType = RouteSubMeshType.Straight;

        [SerializeField]
        protected List<Vector3> m_VertexList = new List<Vector3>();
        [SerializeField]
        protected List<Vector3> m_NormalList = new List<Vector3>();
        [SerializeField]
        protected List<Vector2> m_UVList = new List<Vector2>();
        [SerializeField]
        protected List<int> m_TriangleList = new List<int>();

        protected List<Vector3> m_RealRoutePoints = new List<Vector3>();

        protected void CaculateCoordinate(Vector3 forward , out Vector3 up,out Vector3 right)
        {
            forward = forward.normalized;
            up = Vector3.up;
            right = Vector3.right;
            if (Vector3.Dot(forward, up) == 1)
            {
                up = Vector3.Cross(forward, right).normalized;
                right = Vector3.Cross(up, forward).normalized;
            }
            else
            {
                right = Vector3.Cross(up, forward).normalized;
                up = Vector3.Cross(forward, right).normalized;
            }
        }

        protected void CaculateStraightVertex(Vector3 startPos, Vector3 endPos)
        {
            var dir = (endPos - startPos).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;
            CaculateCoordinate(dir, out up, out right);

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                var rad = 2 * Mathf.PI * i / m_RouteCirclePointCount;
                var point = Mathf.Sin(rad) * m_RouteCircleRadius * right +
                    Mathf.Cos(rad) * m_RouteCircleRadius * up;
                var normal = point.normalized;
                point += startPos;
                Vector2 uv = new Vector2(0, rad * m_RouteCircleRadius);
                m_VertexList.Add(point);
                m_NormalList.Add(normal);
                m_UVList.Add(uv);
            }

            for (int i = 0; i < m_RouteCirclePointCount; i++)
            {
                var rad = 2 * Mathf.PI * i / m_RouteCirclePointCount;
                var point = Mathf.Sin(rad) * m_RouteCircleRadius * right +
                    Mathf.Cos(rad) * m_RouteCircleRadius * up;
                var normal = point.normalized;
                point += endPos;
                Vector2 uv = new Vector2((endPos - startPos).magnitude, rad * m_RouteCircleRadius);
                m_VertexList.Add(point);
                m_NormalList.Add(normal);
                m_UVList.Add(uv);
            }
        }


        public virtual void CaculateMesh() 
        {
            m_VertexList.Clear();
            m_NormalList.Clear();
            m_UVList.Clear();
            m_TriangleList.Clear();
        }

        public virtual void CaculateRealRoutePoints()
        {
            m_RealRoutePoints.Clear();
        }

        protected float[] m_RealPointDises = null;
        public virtual void CaculateDistance()
        {
            m_Distance = 0;
            m_RealPointDises = new float[m_RealRoutePoints.Count];
            m_RealPointDises[0] = 0;
            for (int i =1;i < m_RealRoutePoints.Count;i++)
            {
                var dis = (m_RealRoutePoints[i] - m_RealRoutePoints[i - 1]).magnitude;
                m_Distance += dis;
                m_RealPointDises[i] = m_Distance;
            }
        }

        public Mesh ConvertMesh()
        {
            CaculateMesh();
            var mesh = new Mesh();
            mesh.vertices = m_VertexList.ToArray();
            mesh.normals = m_NormalList.ToArray();
            mesh.triangles = m_TriangleList.ToArray();
            return mesh;
        }

        public MeshData ConvertMesh(int triangleIndex)
        {
            MeshData meshData;
            meshData.m_VertexList = m_VertexList;
            meshData.m_NormalList = m_NormalList;
            meshData.m_UVList = m_UVList;
            List<int> triangleList = new List<int>();
            for(int i =0;i< m_TriangleList.Count;i++)
            {
                triangleList.Add(m_TriangleList[i] + triangleIndex);
            }
            meshData.m_TriangleList = triangleList;
            return meshData;
        }

        #region Sample
        public RoutePoint m_NextPoint = null;
        public RoutePoint m_EnterPoint = null;

        public void EnterSubMesh(RoutePoint enterPoint,Vector2 inputDir)
        {
            m_NextPoint = CaculateNext(enterPoint, inputDir);
            m_EnterPoint = enterPoint;
        }

        public virtual RoutePoint CaculateNext(RoutePoint enterPoint, Vector2 inputDir)
        {
            if(m_Start == enterPoint)
            {
                return m_End;
            }
            else
            {
                return m_Start;
            }
        }

        public virtual (bool,Vector3) SamplePos(float dis)
        {
            if(m_EnterPoint == null)
            {
                return (false,Vector3.zero);
            }

            return (true,m_EnterPoint.m_LocalPos);
        }
        #endregion

    }
}

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

        public int m_RouteCirclePointCount = 20;
        public float m_RouteCircleRadius = 3;

        protected RouteSubMeshType m_RouteMeshType = RouteSubMeshType.Straight;

        [SerializeField]
        protected List<Vector3> m_VertexList = new List<Vector3>();
        [SerializeField]
        protected List<Vector3> m_NormalList = new List<Vector3>();
        [SerializeField]
        protected List<int> m_TriangleList = new List<int>();

        protected List<Vector3> m_RealRoutePoints = new List<Vector3>();

        public virtual void CaculateMesh() 
        {
            m_VertexList.Clear();
            m_NormalList.Clear();
            m_TriangleList.Clear();
        }

        public virtual void CaculateRealRoutePoints()
        {
            m_RealRoutePoints.Clear();
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
            List<int> triangleList = new List<int>();
            for(int i =0;i< m_TriangleList.Count;i++)
            {
                triangleList.Add(m_TriangleList[i] + triangleIndex);
            }
            meshData.m_TriangleList = triangleList;
            return meshData;
        }

    }
}

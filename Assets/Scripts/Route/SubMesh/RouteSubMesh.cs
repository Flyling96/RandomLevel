using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DragonSlay.Route
{
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

        protected RouteSubMeshType m_RouteMeshType = RouteSubMeshType.Straight;

        [SerializeField]
        private List<Vector3> m_VertexList = new List<Vector3>();
        [SerializeField]
        private List<int> m_TriangleList = new List<int>();
      


    }
}

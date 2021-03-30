using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RouteDebugger : MonoBehaviour
    {
        public List<GameObject> m_PointGos = new List<GameObject>();

        public List<int> m_StartPointIndexs = new List<int>();

        public List<int> m_EndPointIndexs = new List<int>();



        public Route m_Route = null;

        public Route Route
        {
            get
            {
                if (m_Route == null)
                {
                    m_Route = new Route(transform.position, transform.rotation);
                }

                return m_Route;
            }
        }

        bool m_EditStart = false;

        private void Awake()
        {
            if(m_Route == null)
            {
                m_Route = new Route(transform.position,transform.rotation);
            }

            m_EditStart = false;
        }

        public void EditState(bool isStart)
        {
            m_EditStart = isStart;
        }

        private void Update()
        {
            Route.UpdateTransform(transform.position, transform.rotation);
        }

        public void GenerateRouteGraph()
        {
            Route.ClearPoints();
            for(int i =0;i< m_PointGos.Count;i++)
            {
                Route.AddPoints(m_PointGos[i].transform.position);
            }

            for(int i = 0; i < m_StartPointIndexs.Count;i++)
            {
                var start = m_StartPointIndexs[i];
                var end = m_EndPointIndexs[i];
                Route.ConnectPoint(start, end);
            }

        }

        public void ConvertMesh()
        {
            var mesh = Route.ConvertSubMeshes();
            if(mesh != null)
            {
                GetComponent<MeshFilter>().sharedMesh = mesh;
            }
        }

        public void CaculateHolePoints()
        {
            //var mesh = Route.CreateHole();
            //if (mesh != null)
            //{
            //    GetComponent<MeshFilter>().sharedMesh = mesh;
            //}
        }

        public int m_EnterGate = 0;
        public float m_SampleDeltaTime = 0.3f;
        private bool m_IsStartRoute = false;
        public void StartRoute()
        {
            Route.StartRoute(m_EnterGate);
            m_IsStartRoute = true;
        }
        public GameObject m_SampleDebugger = null;
        public void SampleRoute()
        {
            if (m_IsStartRoute)
            {
                var res = Route.Sample(m_SampleDeltaTime);
                if(res.Item1)
                {
                    m_SampleDebugger.transform.position = res.Item2;
                }
                else
                {
                    m_IsStartRoute = false;
                }
            }
        }
    }
}

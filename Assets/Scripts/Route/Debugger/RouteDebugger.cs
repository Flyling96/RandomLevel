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

        public void ConvertMesh()
        {
            var mesh = Route.ConvertToMesh();
            if(mesh != null)
            {
                GetComponent<MeshFilter>().sharedMesh = mesh;
            }
        }
    }
}

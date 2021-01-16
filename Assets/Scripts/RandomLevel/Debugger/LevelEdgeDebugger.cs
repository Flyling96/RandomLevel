using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class LevelEdgeDebugger : MonoBehaviour
    {
        [HideInInspector]
        public LevelGraph m_Owner;

        public LevelEdge m_Data;

        MeshFilter m_MeshFilter;

        private void Awake()
        {
            m_MeshFilter = GetComponent<MeshFilter>();    
        }

        public void RefreshMesh()
        {
            if(m_Data != null)
            {
                m_Data.GenerateMesh();
                Mesh mesh = m_Data.ConvertMesh();
                m_MeshFilter.sharedMesh = mesh;
            }
        }
    }
}

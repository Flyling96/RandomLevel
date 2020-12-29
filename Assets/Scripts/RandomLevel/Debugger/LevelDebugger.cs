using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    [ExecuteInEditMode]
    public class LevelDebugger : MonoBehaviour
    {
        public LevelGraph m_LevelGraph;

        public Material m_MeshMaterial;

        public List<GameObject> m_MeshGoList = new List<GameObject>();

       
        public void Awake()
        {
            m_LevelGraph = new LevelGraph();
        }

        public void Clear()
        {
            for (int i = 0; i < m_MeshGoList.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(m_MeshGoList[i]);
                }
                else
                {
                    DestroyImmediate(m_MeshGoList[i]);
                }
            }
            m_MeshGoList.Clear();
        }

        public void GenerateMesh()
        {
            Clear();
            m_LevelGraph.GenerateMesh();
            Mesh[] meshes = m_LevelGraph.DrawMeshes();
            Vector3[] positions = m_LevelGraph.GetMeshPositions();
            for(int i =0;i< meshes.Length;i++)
            {
                var mesh = meshes[i];
                var pos = positions[i];
                GameObject go = new GameObject();
                go.transform.parent = transform;
                go.transform.position = pos;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = m_MeshMaterial;
            }
        }

        public void UpdateMeshPosition()
        {

        }

    }
}

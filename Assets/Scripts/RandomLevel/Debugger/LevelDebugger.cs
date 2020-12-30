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

        public void GenerateAllPanel()
        {
            Clear();
            m_LevelGraph.GenerateMesh();
            Mesh[] meshes = new Mesh[m_LevelGraph.m_LevelMeshList.Count];
            for (int i = 0; i < m_LevelGraph.m_LevelMeshList.Count; i++)
            {
                meshes[i] = m_LevelGraph.m_LevelMeshList[i].FillMesh();
            }

            Vector3[] positions = new Vector3[m_LevelGraph.m_LevelMeshList.Count];
            for (int i = 0; i < m_LevelGraph.m_LevelMeshList.Count; i++)
            {
                positions[i] = m_LevelGraph.m_LevelMeshList[i].m_Position;
            }

            for (int i =0;i< m_LevelGraph.m_LevelMeshList.Count;i++)
            {
                var mesh = meshes[i];
                var pos = positions[i];
                var meshData = m_LevelGraph.m_LevelMeshList[i];
                string goName;
                if (meshData is LevelPanel levelPanel && m_LevelGraph.m_RoomPanelList.Contains(levelPanel))
                {
                    goName = "Main Panel";
                }
                else
                {
                    goName = "Minor Panel";
                }

                m_MeshGoList.Add(CreateMeshGameObject(mesh,pos,goName,Random.ColorHSV()));
            }
        }

        GameObject CreateMeshGameObject(Mesh mesh,Vector3 pos,string name,Color color)
        {
            GameObject go = new GameObject();
            go.name = name;
            go.transform.parent = transform;
            go.transform.position = pos;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var render = go.AddComponent<MeshRenderer>();
            render.sharedMaterial = new Material(m_MeshMaterial);
            render.sharedMaterial.SetColor("_Color", color);
            return go;
        }

        public void FilterMinorPanel()
        {
            for(int i = m_MeshGoList.Count - 1; i > -1; i--)
            {
                var go = m_MeshGoList[i];
                if(go.name.Contains("Minor"))
                {
                    m_MeshGoList.RemoveAt(i);
                    if (Application.isPlaying)
                    {
                        Destroy(go);
                    }
                    else
                    {
                        DestroyImmediate(go);
                    }
                }
            }
        }

        public void GenerateEdge()
        {
            m_LevelGraph.GenerateEdge();
            Mesh[] meshes = new Mesh[m_LevelGraph.m_EdgeList.Count];
            for (int i = 0; i < m_LevelGraph.m_EdgeList.Count; i++)
            {
                meshes[i] = m_LevelGraph.m_EdgeList[i].FillMesh();
            }

            Vector3[] positions = new Vector3[m_LevelGraph.m_EdgeList.Count];
            for (int i = 0; i < m_LevelGraph.m_EdgeList.Count; i++)
            {
                positions[i] = m_LevelGraph.m_EdgeList[i].m_Position;
            }

            for (int i = 0; i < m_LevelGraph.m_EdgeList.Count; i++)
            {
                var mesh = meshes[i];
                var pos = positions[i];
                m_MeshGoList.Add(CreateMeshGameObject(mesh, pos, "Edge",Color.red));
            }
        }

        public void CollsionSimulate(int simulateCount)
        {
            m_LevelGraph.CollsionSimulate(simulateCount);
            UpdateMeshPosition();
        }

        public void UpdateMeshPosition()
        {
            Vector3[] positions = m_LevelGraph.GetMeshPositions();
            for(int i=0;i< m_MeshGoList.Count;i++)
            {
                m_MeshGoList[i].transform.position = positions[i];
            }
        }

        public void EdgeMeshTest()
        {
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(10, 10);
            Vector2[] mids = new Vector2[3] { new Vector2(1, 2), new Vector2(5, 3), new Vector2(2, 3) };
            mids = new Vector2[0];
            LevelEdge edge = new LevelEdge(start,end,mids,1);
            edge.GenerateMesh();
            Mesh edgeMesh = edge.FillMesh();

            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.transform.position = Vector3.zero;
            go.AddComponent<MeshFilter>().sharedMesh = edgeMesh;
            var render = go.AddComponent<MeshRenderer>();
            render.sharedMaterial = new Material(m_MeshMaterial);

            m_MeshGoList.Add(go);
        }

    }
}

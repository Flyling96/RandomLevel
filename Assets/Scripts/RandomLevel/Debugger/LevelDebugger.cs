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

        public Dictionary<LevelMesh, GameObject> m_MeshGoDic = new Dictionary<LevelMesh, GameObject>();

       
        public void Awake()
        {
            m_LevelGraph = new LevelGraph();
        }

        public void Clear()
        {
            foreach (var go in m_MeshGoDic.Values)
            {
                if (Application.isPlaying)
                {
                    Destroy(go);
                }
                else
                {
                    DestroyImmediate(go);
                }
            }
            m_MeshGoDic.Clear();
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

                m_MeshGoDic.Add(meshData,CreateMeshGameObject(mesh,pos,goName,Random.ColorHSV()));
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
            List<LevelMesh> removeKeys = new List<LevelMesh>();
            foreach (var keyValue in m_MeshGoDic)
            {
                var meshData = keyValue.Key;
                var go = keyValue.Value;

                if(!(meshData is LevelPanel levelPanel && m_LevelGraph.m_RoomPanelList.Contains(levelPanel)))
                {
                    removeKeys.Add(meshData);
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

            for(int i =0;i< removeKeys.Count;i++)
            {
                m_MeshGoDic.Remove(removeKeys[i]);
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
                var meshData = m_LevelGraph.m_EdgeList[i];
                var mesh = meshes[i];
                var pos = positions[i];
                m_MeshGoDic.Add(meshData,CreateMeshGameObject(mesh, pos, "Edge",Color.red));
            }
        }

        public void GenerateVoxel()
        {
            foreach(var keyValue in m_MeshGoDic)
            {
                var meshData = keyValue.Key;
                var go = keyValue.Value;
                for(int i =0;i< go.transform.childCount;i++)
                {
                    var child = go.transform.GetChild(i);
                    if (Application.isPlaying)
                    {
                        Destroy(child);
                    }
                    else
                    {
                        DestroyImmediate(child);
                    }
                }

                meshData.GenerateVoxel(1);
                for(int i =0;i< meshData.m_Voxels.Count;i++)
                {
                    var voxel = meshData.m_Voxels[i];
                    var pos = voxel.m_Position;
                    Mesh mesh = voxel.FillMesh();
                    Color color = Color.white;
                    if(voxel is LevelCell levelCell)
                    {
                        if(!levelCell.m_IsShow)
                        {
                            color = Color.black;
                        }
                    }
                    var cell =  CreateMeshGameObject(mesh, pos, "Cell", color);
                    cell.transform.parent = go.transform;
                }
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
            foreach (KeyValuePair<LevelMesh, GameObject> keyValue in m_MeshGoDic)
            {
                var meshData = keyValue.Key;
                var go = keyValue.Value;
                go.transform.position = meshData.m_Position;
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

            m_MeshGoDic.Add(edge,go);
        }

    }
}

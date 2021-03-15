using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;
using DragonSlay.RandomLevel.Gameplay;

namespace DragonSlay.RandomLevel
{
    [ExecuteInEditMode]
    public class LevelDebugger : MonoBehaviour
    {
        public LevelGraph m_SceneLevel;

        public Level m_GameplayLevel;

        public Material m_MeshMaterial;

        public Material m_VoxelMeshMaterial;

        public Dictionary<LevelMesh, GameObject> m_MeshGoDic = new Dictionary<LevelMesh, GameObject>();

       
        public void Awake()
        {
            m_SceneLevel = new LevelGraph();
            m_GameplayLevel = new Level();
        }

        public void Clear()
        {
            for(int i = transform.childCount -1; i > -1;i--)
            {
                var go = transform.GetChild(i).gameObject;
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
            m_GameplayLevel?.Clear();
        }

        public void GenerateAllPanel()
        {
            Clear();
            m_SceneLevel.GenerateMesh();
            Mesh[] meshes = new Mesh[m_SceneLevel.m_LevelMeshList.Count];
            for (int i = 0; i < m_SceneLevel.m_LevelMeshList.Count; i++)
            {
                meshes[i] = m_SceneLevel.m_LevelMeshList[i].ConvertMesh();
            }

            Vector3[] positions = new Vector3[m_SceneLevel.m_LevelMeshList.Count];
            for (int i = 0; i < m_SceneLevel.m_LevelMeshList.Count; i++)
            {
                positions[i] = m_SceneLevel.m_LevelMeshList[i].m_Position;
            }

            for (int i =0;i< m_SceneLevel.m_LevelMeshList.Count;i++)
            {
                var mesh = meshes[i];
                var pos = positions[i];
                var meshData = m_SceneLevel.m_LevelMeshList[i];
                string goName;
                if (meshData is LevelPanel levelPanel && m_SceneLevel.m_RoomList.Contains(levelPanel))
                {
                    goName = "Main Panel";
                }
                else
                {
                    goName = "Minor Panel";
                }

                var block = new MaterialPropertyBlock();
                block.SetColor("_Color", Random.ColorHSV());
                var go = CreateMeshGameObject(mesh, pos, goName, m_MeshMaterial, block);
                var debugger = go.AddComponent<LevelPanelDebugger>();
                debugger.m_Owner = m_SceneLevel;
                debugger.m_Data = meshData as LevelPanel;

                m_MeshGoDic.Add(meshData, go);
            }
        }

        GameObject CreateMeshGameObject(Mesh mesh,Vector3 pos,string name, Material mat,MaterialPropertyBlock block)
        {
            GameObject go = new GameObject();
            go.name = name;
            go.transform.parent = transform;
            go.transform.position = pos;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var render = go.AddComponent<MeshRenderer>();
            render.sharedMaterial = new Material(mat);
            render.SetPropertyBlock(block);
            return go;
        }

        public void FilterMinorPanel()
        {
            List<LevelMesh> removeKeys = new List<LevelMesh>();
            foreach (var keyValue in m_MeshGoDic)
            {
                var meshData = keyValue.Key;
                var go = keyValue.Value;

                if(!(meshData is LevelPanel levelPanel && m_SceneLevel.m_RoomList.Contains(levelPanel)))
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
            m_SceneLevel.GenerateEdge();
            Mesh[] meshes = new Mesh[m_SceneLevel.m_EdgeList.Count];
            for (int i = 0; i < m_SceneLevel.m_EdgeList.Count; i++)
            {
                meshes[i] = m_SceneLevel.m_EdgeList[i].ConvertMesh();
            }

            Vector3[] positions = new Vector3[m_SceneLevel.m_EdgeList.Count];
            for (int i = 0; i < m_SceneLevel.m_EdgeList.Count; i++)
            {
                positions[i] = m_SceneLevel.m_EdgeList[i].m_Position;
            }

            for (int i = 0; i < m_SceneLevel.m_EdgeList.Count; i++)
            {
                var meshData = m_SceneLevel.m_EdgeList[i];
                var mesh = meshes[i];
                var pos = positions[i];
                var block = new MaterialPropertyBlock();
                block.SetColor("_Color", Color.red);
                var go = CreateMeshGameObject(mesh, pos, "Edge", m_MeshMaterial, block);
                var debugger = go.AddComponent<LevelEdgeDebugger>();
                debugger.m_Owner = m_SceneLevel;
                debugger.m_Data = meshData;
                m_MeshGoDic.Add(meshData, go);
            }
        }

        public void GenerateDoor()
        {
            m_SceneLevel.GenerateDoor();
            Mesh[] meshes = new Mesh[m_SceneLevel.m_DoorList.Count];
            Vector3[] positions = new Vector3[m_SceneLevel.m_DoorList.Count];
            for (int i = 0; i < m_SceneLevel.m_DoorList.Count; i++)
            {
                meshes[i] = m_SceneLevel.m_DoorList[i].ConvertMesh();
                positions[i] = m_SceneLevel.m_DoorList[i].m_Position + Vector3.up * 0.2f;
            }

            for (int i = 0; i < m_SceneLevel.m_DoorList.Count; i++)
            {
                var meshData = m_SceneLevel.m_DoorList[i];
                var mesh = meshes[i];
                var pos = positions[i];
                var block = new MaterialPropertyBlock();
                block.SetColor("_Color", Color.white);
                var go = CreateMeshGameObject(mesh, pos, "Door", m_MeshMaterial, block);
                var debugger = go.AddComponent<LevelPanelDebugger>();
                debugger.m_Owner = m_SceneLevel;
                debugger.m_Data = meshData;
                m_MeshGoDic.Add(meshData, go);
            }
        }

        Mesh m_VoxelMesh = null;

        public void GenerateVoxel()
        {
            float time = Time.realtimeSinceStartup;
            Vector3 pos = Vector3.zero;
            m_SceneLevel.GenerateVoxel();
            Debug.Log(Time.realtimeSinceStartup - time);
            time = Time.realtimeSinceStartup;
            m_VoxelMesh =  m_SceneLevel.GenerateGraphMesh(ref pos);
            Debug.Log(Time.realtimeSinceStartup - time);
            time = Time.realtimeSinceStartup;
            var colors = m_SceneLevel.GenerateGraphColors();
            m_VoxelMesh.colors = colors;
            var block = new MaterialPropertyBlock();
            var graph = CreateMeshGameObject(m_VoxelMesh, pos, "Graph", m_VoxelMeshMaterial, block);
            graph.transform.position += Vector3.up * 0.3f;

        }
    

        public void TempRefreshColor()
        {
            foreach(var keyValue in m_MeshGoDic)
            {
                var levelMesh = keyValue.Key;
                var go = keyValue.Value;
                if(levelMesh is LevelPanel levelPanel)
                {
                    if(levelPanel.Shape != null)
                    {
                        go.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", 
                            levelPanel.Shape.m_Pole == 0 ? Color.red : Color.green);
                    }
                }
            }
        }

        public void GrahamScan()
        {
            var points = new Vector2[m_SceneLevel.m_RoomList.Count];
            for(int i =0;i< points.Length; i++)
            {
                points[i] = m_SceneLevel.m_RoomList[i].m_PanelPosition;
            }

            var data = GeometryHelper.CaculatePointsBoundingSphere2D(points);
            if(data.Item2 > 0)
            {
                CirclePanel panel = new CirclePanel(data.Item2, Vector2.zero,new Vector3(data.Item1.x,0,data.Item1.y));
                panel.GenerateMesh();
                Mesh mesh = panel.ConvertMesh();
                var block = new MaterialPropertyBlock();
                block.SetColor("_Color", Random.ColorHSV());
                var go = CreateMeshGameObject(mesh, new Vector3(data.Item1.x, 0, data.Item1.y), "GrahamScan", m_MeshMaterial, block);
                var debugger = go.AddComponent<LevelPanelDebugger>();
                debugger.m_Owner = m_SceneLevel;
                debugger.m_Data = panel as LevelPanel;
            }
        }

        public void RefreshColor()
        {
            var colors = m_SceneLevel.GenerateGraphColors();
            m_VoxelMesh.colors = colors;
        }

        public void CollsionSimulate(int simulateCount)
        {
            m_SceneLevel.CollsionSimulate(simulateCount);
            UpdateMeshPosition();
        }

        public void UpdateMeshPosition()
        {
            Vector3[] positions = m_SceneLevel.GetMeshPositions();
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
            Mesh edgeMesh = edge.ConvertMesh();

            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.transform.position = Vector3.zero;
            go.AddComponent<MeshFilter>().sharedMesh = edgeMesh;
            var render = go.AddComponent<MeshRenderer>();
            render.sharedMaterial = new Material(m_MeshMaterial);

            m_MeshGoDic.Add(edge,go);
        }

        #region Gameplay
        public void GenerateGameplayLevel()
        {
            if(m_SceneLevel == null || m_GameplayLevel == null)
            {
                return;
            }
            m_GameplayLevel.Init(m_SceneLevel);
            RefreshColor();
        
        }

        public void InitLevelStartEnd()
        {
            if (m_SceneLevel == null || m_GameplayLevel == null)
            {
                return;
            }
            m_GameplayLevel.InitStartEnd();
        }
        #endregion

    }
}

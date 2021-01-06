using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace DragonSlay.RandomLevel.Scene
{
    [Serializable]
    public class LevelGraph
    {
        private readonly int m_tile_size = 4;

        [SerializeField, Header("随机点水平分布")]
        private int m_Width = 400;

        [SerializeField, Header("随机点垂直分布")]
        private int m_Height = 400;

        [SerializeField, Header("初始点数量")]
        private int m_InitCount = 30;

        [SerializeField, Header("保留几何体数量")]
        private int m_NeedMainPanel = 5;

        [SerializeField, Header("几何体筛选，以面积判定")]
        private int m_RoomFilter = 2500;

        [SerializeField, Range(20, 200), Header("随机宽高范围")]
        private int m_PointRandomRange = 60;

        [SerializeField, Range(0, 1f), Header("融合三角剖分百分比")]
        private float m_MixPersents = 0.15f;

        [SerializeField, Header("Cell的大小")]
        public int m_CellSize = 1;

        public List<LevelMesh> m_LevelMeshList = new List<LevelMesh>();

        public List<LevelPanel> m_PanelList = new List<LevelPanel>();

        public List<LevelEdge> m_EdgeList = new List<LevelEdge>();

        public void Clear()
        {
            m_LevelMeshList.Clear();
            m_PanelList.Clear();
        }

        public void GenerateMesh()
        {
            Clear();

            Vector2[] allRect = new Vector2[m_InitCount];
            m_RoomFilter = Mathf.Min((m_PointRandomRange - 10) * (m_PointRandomRange - 10), m_RoomFilter);
            while (true)
            {
                int main_count = 0;
                for (int i = 0; i < m_InitCount; i++)
                {
                    Vector2 rect;
                    if (main_count == m_NeedMainPanel)
                    {
                        rect = new Vector2(Random.Range(20, Mathf.Sqrt(m_RoomFilter)), Random.Range(20, Mathf.Sqrt(m_RoomFilter)));
                    }
                    else
                    {
                        rect = new Vector2(Random.Range(20, m_PointRandomRange), Random.Range(20, m_PointRandomRange));
                    }
                    if (rect.x * rect.y > m_RoomFilter)
                    {
                        main_count++;
                    }
                    allRect[i] = rect;
                }
                if (main_count >= m_NeedMainPanel)
                {
                    break;
                }
            }

            for (int i = 0; i < allRect.Length; i++)
            {
                var pos = GetRandomPointInEllipse(m_Width, m_Height);
                var rect = allRect[i];
                RectPanel rectPanel = new RectPanel(rect.x, rect.y, Vector2.zero, pos);
                rectPanel.GenerateMesh();
                m_LevelMeshList.Add(rectPanel);
                if(rectPanel.m_Acreage > m_RoomFilter)
                {
                    m_PanelList.Add(rectPanel);
                }
            }

        }

        private float Roundm(float n, float m)
        {
            return Mathf.Floor(((n + m - 1) / m)) * m;
        }

        private Vector3 GetRandomPointInEllipse(int ellipse_width = 400, int ellipse_height = 400)
        {
            float t = 2 * Mathf.PI * Random.Range(0, 0.9999f);
            float u = Random.Range(0, 0.9999f) + Random.Range(0, 0.9999f);
            float r = u > 1 ? 2 - u : u;
            return new Vector3(Roundm(ellipse_width * r * Mathf.Cos(t) / 2, m_tile_size),0,
                   Roundm(ellipse_height * r * Mathf.Sin(t) / 2, m_tile_size));
        }

        public Mesh[] DrawMeshes()
        {
            Mesh[] result = new Mesh[m_LevelMeshList.Count];
            for(int i =0;i< m_LevelMeshList.Count;i++)
            {
                result[i] = m_LevelMeshList[i].ConvertMesh();
            }
            return result;
        }

        public Vector3[] GetMeshPositions()
        {
            Vector3[] result = new Vector3[m_LevelMeshList.Count];
            for (int i = 0; i < m_LevelMeshList.Count; i++)
            {
                result[i] = m_LevelMeshList[i].m_Position;
            }
            return result;
        }

        public void CollsionSimulate(int simulateCount)
        {
            List<Polygon> m_PolygonList = new List<Polygon>();

            for(int i = 0;i<m_LevelMeshList.Count;i++)
            {
                var levelMesh = m_LevelMeshList[i];
                if(levelMesh is LevelPanel levelPanel)
                {
                    m_PolygonList.Add(levelPanel.ToPolygon());
                }
            }

            SeparatingAxisAlgorithm algorithm = new SeparatingAxisAlgorithm();
            bool isCollide = false;
            while (simulateCount > 0)
            {
                for (int i = 0; i < m_PolygonList.Count; i++)
                {
                    var polygon0 = m_PolygonList[i];
                    for (int j = i + 1; j < m_PolygonList.Count; j++)
                    {
                        var polygon1 = m_PolygonList[j];
                        if (algorithm.SeparatingAxis(polygon0, polygon1))
                        {
                            isCollide = true;
                        }
                    }
                }

                if (!isCollide)
                {
                    break;
                }
                simulateCount--;
            }

            for (int i = 0; i < m_LevelMeshList.Count; i++)
            {
                var levelMesh = m_LevelMeshList[i];
                if (levelMesh is LevelPanel levelPanel)
                {
                    var polygon = m_PolygonList[i];
                    levelPanel.SetPosition(polygon.m_Position);
                }
            }

        }

        public void GenerateEdge()
        {
            m_EdgeList.Clear();
            //三角剖分
            List<UVertex2D> vertexs = new List<UVertex2D>();
            for (int i = 0; i < m_PanelList.Count; i++)
            {
                vertexs.Add(new UVertex2D(i, m_PanelList[i].m_PanelPosition + m_PanelList[i].m_Center));
            }
            var delaunayResult = UDelaunayBest.GetTriangles2D(vertexs);

            Dictionary<int, UVertex2D> dict = new Dictionary<int, UVertex2D>();
            for (int i = 0; i < delaunayResult.Vertexes.Count; i++)
            {
                if (dict.ContainsKey(delaunayResult.Vertexes[i].Id))
                {
                    continue;
                }
                dict.Add(delaunayResult.Vertexes[i].Id, delaunayResult.Vertexes[i]);
            }
            var edges = new List<UEdge2D>();
            for (int i = vertexs.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    edges.Add(new UEdge2D(vertexs[i], vertexs[j]));
                }
            }
            //最小生成树
            var spanningTreeEdges = SpanningTree.Kruskal(dict, edges);
            //delaunay中去重排序
            for (int i = 0; i < spanningTreeEdges.Count; i++)
            {
                for (int j = delaunayResult.Edges.Count - 1; j >= 0; j--)
                {
                    if (spanningTreeEdges[i].IsEquals(delaunayResult.Edges[j]))
                    {
                        delaunayResult.Edges.RemoveAt(j);
                    }
                }
            }
            delaunayResult.Edges.Sort((a, b) =>
            {
                if (a.Distance > b.Distance)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            });

            //添加边
            int count = (int)(delaunayResult.Edges.Count * m_MixPersents);
            for (int i = 0; i < count; i++)
            {
                spanningTreeEdges.Add(delaunayResult.Edges[i]);
            }

            for (int i =0;i< spanningTreeEdges.Count;i++)
            {
                var edge = spanningTreeEdges[i];
                var levelEdge = new LevelEdge(edge,5);
                levelEdge.GenerateMesh();
                m_EdgeList.Add(levelEdge);
            }

        }

        public Dictionary<Vector2, LevelCell> m_LevelCellDic = new Dictionary<Vector2, LevelCell>();
        Vector2 m_StartPoint;
        Vector3 m_Right = new Vector3(1, 0, 0);
        Vector3 m_Up = new Vector3(0, 0, 1);

        public void GenerateVoxel()
        {
            int voxelSize = m_CellSize;
            m_LevelCellDic.Clear();
            List<AABoundingBox2D> aabb2DList = new List<AABoundingBox2D>();
            List<Vector2> voxelCenterList = new List<Vector2>();
            HashSet<Vector2> voxelCenterSet = new HashSet<Vector2>();
            for (int i =0;i< m_PanelList.Count;i++)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("Voxel", string.Format("VoxelRoom : {0}/{1}", i, m_PanelList.Count), (float)i / m_PanelList.Count);
#endif
                var room = m_PanelList[i];
                Vector2 pos = room.CalculateVoxelMeshPos2D(voxelSize);
                var aabb2D = room.GetAABB2D(m_CellSize) + pos;
                int minX = (int)aabb2D.m_Min.x;
                int minY = (int)aabb2D.m_Min.y;
                int maxX = (int)aabb2D.m_Max.x;
                int maxY = (int)aabb2D.m_Max.y;

                bool isStart = false;
                for (int y = minY; y < maxY + 1; y += voxelSize)
                {
                    for (int x = minX; x < maxX + 1; x += voxelSize)
                    {
                        LevelCell cell = null;
                        Vector2 cellCenter = new Vector2(x, y);
                        //XXX:Dictionary 查询消耗较大，可以考虑用数组替代
                        if (!m_LevelCellDic.TryGetValue(cellCenter,out cell))
                        {
                            cell = new LevelCell(cellCenter, room.m_Right, room.m_Up, voxelSize);
                            m_LevelCellDic.Add(cellCenter, cell);
                        }

                        if (cell.IsInMesh(room))
                        {
                            cell.m_SceneCell.MaskCell(SceneCellType.Room);
                            cell.m_GameplayCell.m_Walkable = true;
                            if(!isStart)
                            {
                                room.m_CellStart = cell.m_Center;
                                isStart = true;
                            }
                        }
                    }
                }
                aabb2DList.Add(m_PanelList[i].GetAABB2D(voxelSize) + pos);
            }

            for (int i = 0; i < m_EdgeList.Count; i++)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("Voxel", string.Format("VoxelEdge : {0}/{1}", i, m_EdgeList.Count), (float)i / m_EdgeList.Count);
#endif
                var edge = m_EdgeList[i];
                Vector2 pos = edge.CalculateVoxelMeshPos2D(voxelSize);
                var aabb2Ds = edge.GetAABB2Ds(voxelSize);
                bool isStart = false;
                for (int j =0;j<aabb2Ds.Length;j++)
                {
                    var aabb2D = aabb2Ds[j] + pos;
                    int minX = (int)aabb2D.m_Min.x;
                    int minY = (int)aabb2D.m_Min.y;
                    int maxX = (int)aabb2D.m_Max.x;
                    int maxY = (int)aabb2D.m_Max.y;

                    for (int y = minY; y < maxY + 1; y += voxelSize)
                    {
                        for (int x = minX; x < maxX + 1; x += voxelSize)
                        {
                            LevelCell cell = null;
                            Vector2 cellCenter = new Vector2(x, y);
                            if (!m_LevelCellDic.TryGetValue(cellCenter, out cell))
                            {
                                cell = new LevelCell(cellCenter, edge.m_Right, edge.m_Up, voxelSize);
                                m_LevelCellDic.Add(cellCenter, cell);
                            }

                            if (cell.IsInMesh(edge))
                            {
                                cell.m_SceneCell.MaskCell(SceneCellType.Corridor);
                                cell.m_GameplayCell.m_Walkable = true;
                                if (!isStart)
                                {
                                    edge.m_CellStart = cell.m_Center;
                                    isStart = true;
                                }
                            }
                        }
                    }
                    aabb2DList.Add(aabb2D);
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

            AABoundingBox2D aabbBig = new AABoundingBox2D(aabb2DList.ToArray());
            aabbBig.m_Min.x = Mathf.FloorToInt(aabbBig.m_Min.x / voxelSize) * voxelSize;
            aabbBig.m_Min.y = Mathf.FloorToInt(aabbBig.m_Min.y / voxelSize) * voxelSize;
            aabbBig.m_Max.x = Mathf.CeilToInt(aabbBig.m_Max.x / voxelSize) * voxelSize ;
            aabbBig.m_Max.y = Mathf.CeilToInt(aabbBig.m_Max.y / voxelSize) * voxelSize;

            m_StartPoint = aabbBig.m_Min;

        }

        public Mesh GenerateGraphMesh(ref Vector3 startPos)
        {
            startPos = m_StartPoint.x * m_Right + m_StartPoint.y * m_Up;
            List<Vector3> vertexList = new List<Vector3>();
            List<int> triangleList = new List<int>();
            foreach (var keyValue in m_LevelCellDic)
            {
                var cell = keyValue.Value;
                cell.FillMesh(vertexList, triangleList, startPos);
            }

            float time = Time.realtimeSinceStartup;
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertexList.ToArray();
            mesh.triangles = triangleList.ToArray();
            Debug.Log(Time.realtimeSinceStartup - time);
            time = Time.realtimeSinceStartup;

            return mesh;
        }

        public Color[] GenerateGraphColors()
        {
            Color[] tempColors = new Color[4] { Color.black ,Color.red, Color.green, Color.blue };
            Color[] graphColors = new Color[m_LevelCellDic.Count * 5];

            foreach(var cell in m_LevelCellDic.Values)
            {
                Color color = Color.black;
                for(int i =0;i< (int)SceneCellType.Max;i++)
                {
                    if(cell.m_SceneCell.IsMaskCell((SceneCellType)i))
                    {
                        color += tempColors[i];
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    int index = cell.subMeshCenterIndex + i;
                    graphColors[index] = color;
                }
            }

            return graphColors;
        }

        public Color[] GenerateNewColor()
        {
            Color[] graphColors = new Color[m_LevelCellDic.Count * 5];
            for(int i =0;i< graphColors.Length;i++)
            {
                graphColors[i] = Color.blue;
            }
            return graphColors;
        }

        public Vector3 CalculateVoxelMeshPos(LevelMesh mesh)
        {
            return mesh.CalculateVoxelMeshPos( m_CellSize);
        }


    }
}

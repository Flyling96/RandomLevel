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

        public List<LevelPanel> m_RoomList = new List<LevelPanel>();

        public List<LevelEdge> m_EdgeList = new List<LevelEdge>();

        public List<LevelPanel> m_DoorList = new List<LevelPanel>();

        public void Clear()
        {
            m_LevelMeshList.Clear();
            m_RoomList.Clear();
        }

        public void GenerateMesh()
        {
            Clear();

            m_NeedMainPanel = Mathf.Min(m_NeedMainPanel, m_InitCount);
            Vector2[] allRect = new Vector2[m_InitCount];
            m_RoomFilter = Mathf.Min((m_PointRandomRange - 10) * (m_PointRandomRange - 10), m_RoomFilter);
            int count = 0;
            int index = 0;
            while (true)
            {
                if (count == m_NeedMainPanel)
                {
                    break;
                }

                Vector2 rect = new Vector2(Random.Range(20, m_PointRandomRange), Random.Range(20, m_PointRandomRange));
                if (rect.x * rect.y >= m_RoomFilter)
                {
                    allRect[index] = rect;
                    index++;
                    count++;
                }
            }

            count = 0;
            while (true)
            {
                if (count == m_InitCount - m_NeedMainPanel)
                {
                    break;
                }

                Vector2 rect = new Vector2(Random.Range(20, m_PointRandomRange), Random.Range(20, m_PointRandomRange));
                if (rect.x * rect.y < m_RoomFilter)
                {
                    allRect[index] = rect;
                    index++;
                    count++;
                }
            }


            for (int i = 0; i < allRect.Length; i++)
            {
                var pos = GetRandomPointInEllipse(m_Width, m_Height);
                var rect = allRect[i];
                int type = Random.Range(0, 2);
                LevelPanel levelPanel = null;
                if (type == 0)
                {
                    levelPanel = new RectPanel(rect.x, rect.y, Vector2.zero, pos, Random.Range(0,Mathf.PI));
                }
                else if(type == 1)
                {
                    levelPanel = new CirclePanel(Mathf.Max(rect.x, rect.y) / 2.0f, Vector2.zero, pos);
                }
                levelPanel.GenerateMesh();
                m_LevelMeshList.Add(levelPanel);
                if(rect.x * rect.y > m_RoomFilter)
                {
                    m_RoomList.Add(levelPanel);
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
            List<Shape> shapeList = new List<Shape>();

            for(int i = 0;i<m_LevelMeshList.Count;i++)
            {
                var levelMesh = m_LevelMeshList[i];
                if(levelMesh is LevelPanel levelPanel)
                {
                    shapeList.Add(levelPanel.Shape);
                }
            }

            bool isCollide = false;
            while (simulateCount > 0)
            {
                for (int i = 0; i < shapeList.Count; i++)
                {
                    var shape0 = shapeList[i];
                    for (int j = i + 1; j < shapeList.Count; j++)
                    {
                        var shape1 = shapeList[j];
                        if (GeometryHelper.SeparatingAxis(shape0, shape1))
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
                    var polygon = shapeList[i];
                    levelPanel.SetPosition(polygon.m_Position);
                }
            }

        }

        public void GenerateEdge()
        {
            m_EdgeList.Clear();
            //三角剖分
            List<UVertex2D> vertexs = new List<UVertex2D>();
            List<Shape> shapeList = new List<Shape>();
            for (int i = 0; i < m_RoomList.Count; i++)
            {
                vertexs.Add(new UVertex2D(i, m_RoomList[i].m_PanelPosition + m_RoomList[i].m_Center));
                shapeList.Add(m_RoomList[i].Shape);
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
                var startPolygon = shapeList[edge.Point0.Id];
                var endPolygon = shapeList[edge.Point1.Id];
                //var levelEdge = new LevelEdge(edge,5);
                var levelEdge = new LevelEdge(startPolygon, endPolygon, shapeList, 5);
                levelEdge.GenerateMesh();
                m_EdgeList.Add(levelEdge);
            }

        }

        public void GenerateDoor()
        {
            m_DoorList.Clear();
            for(int i =0;i < m_EdgeList.Count;i++)
            {
                var edge = m_EdgeList[i];
                var doors = edge.GenerateDoor(3);
                m_DoorList.AddRange(doors);
            }
        }

        public Dictionary<Vector3, LevelCell> m_LevelCellDic = new Dictionary<Vector3, LevelCell>();
        Vector2 m_StartPoint;
        Vector3 m_Right = new Vector3(1, 0, 0);
        Vector3 m_Up = new Vector3(0, 0, 1);

        void GeneratePanelVoxel(LevelPanel panel, AABoundingBox2D aabb2D, SceneCellType type, bool onlyMask = false)
        {
            int minX = (int)aabb2D.m_Min.x;
            int minY = (int)aabb2D.m_Min.y;
            int maxX = (int)aabb2D.m_Max.x;
            int maxY = (int)aabb2D.m_Max.y;

            float centerDis = float.MaxValue;
            for (int y = minY; y < maxY + 1; y += m_CellSize)
            {
                for (int x = minX; x < maxX + 1; x += m_CellSize)
                {
                    LevelCell cell = null;
                    Vector2 cellCenter = new Vector2(x, y);
                    Vector3 cellPos = cellCenter.x * panel.m_Right + cellCenter.y * panel.m_Up;
                    //XXX:Dictionary 查询消耗较大，可以考虑用数组替代
                    if (!m_LevelCellDic.TryGetValue(cellPos, out cell))
                    {
                        cell = new LevelCell(cellCenter, panel.m_Right, panel.m_Up, m_CellSize);
                        m_LevelCellDic.Add(cellPos, cell);
                    }

                    if (cell.IsInMesh(panel))
                    {
                        if (onlyMask)
                        {
                            cell.m_SceneCell.OnlyMaskCell(type);
                        }
                        else
                        {
                            cell.m_SceneCell.MaskCell(type);
                        }
                        cell.m_GameplayCell.m_Walkable = true;
                        float dis = Vector3.Distance(cell.m_Position, panel.m_Position);
                        if (dis < centerDis)
                        {
                            centerDis = dis;
                            panel.m_CellStart = cell.m_Position;
                        }
                    }
                }
            }
        }

        public void GenerateVoxel()
        {
            m_LevelCellDic.Clear();
            List<AABoundingBox2D> aabb2DList = new List<AABoundingBox2D>();
            List<Vector2> voxelCenterList = new List<Vector2>();
            HashSet<Vector2> voxelCenterSet = new HashSet<Vector2>();
            for (int i =0;i< m_RoomList.Count;i++)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("Voxel", string.Format("VoxelRoom : {0}/{1}", i, m_RoomList.Count), (float)i / m_RoomList.Count);
#endif
                var room = m_RoomList[i];
                Vector2 pos = room.CalculateVoxelMeshPos2D(m_CellSize);
                var aabb2D = room.GetAABB2D(m_CellSize) + pos;
                GeneratePanelVoxel(room, aabb2D,SceneCellType.Room);
                aabb2DList.Add(aabb2D);
            }

            for (int i = 0; i < m_DoorList.Count; i++)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("Voxel", string.Format("VoxelDoor : {0}/{1}", i, m_DoorList.Count), (float)i / m_DoorList.Count);
#endif
                var door = m_DoorList[i];
                Vector2 pos = door.CalculateVoxelMeshPos2D(m_CellSize);
                var aabb2D = door.GetAABB2D(m_CellSize) + pos;
                GeneratePanelVoxel(door, aabb2D, SceneCellType.Door);
                aabb2DList.Add(aabb2D);
            }

            for (int i = 0; i < m_EdgeList.Count; i++)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayProgressBar("Voxel", string.Format("VoxelEdge : {0}/{1}", i, m_EdgeList.Count), (float)i / m_EdgeList.Count);
#endif
                var edge = m_EdgeList[i];
                Vector2 pos = edge.CalculateVoxelMeshPos2D(m_CellSize);
                var aabb2Ds = edge.GetAABB2Ds(m_CellSize);
                bool isStart = false;
                for (int j =0;j<aabb2Ds.Length;j++)
                {
                    var aabb2D = aabb2Ds[j] + pos;
                    int minX = (int)aabb2D.m_Min.x;
                    int minY = (int)aabb2D.m_Min.y;
                    int maxX = (int)aabb2D.m_Max.x;
                    int maxY = (int)aabb2D.m_Max.y;

                    for (int y = minY; y < maxY + 1; y += m_CellSize)
                    {
                        for (int x = minX; x < maxX + 1; x += m_CellSize)
                        {
                            LevelCell cell = null;
                            Vector2 cellCenter = new Vector2(x, y);
                            Vector3 cellPos = cellCenter.x * edge.m_Right + cellCenter.y * edge.m_Up;
                            if (!m_LevelCellDic.TryGetValue(cellPos, out cell))
                            {
                                cell = new LevelCell(cellCenter, edge.m_Right, edge.m_Up, m_CellSize);
                                m_LevelCellDic.Add(cellPos, cell);
                            }

                            if (cell.IsInMesh(edge) && !cell.m_SceneCell.IsMaskCell(SceneCellType.Room))
                            {
                                cell.m_SceneCell.MaskCell(SceneCellType.Corridor);
                                cell.m_GameplayCell.m_Walkable = true;
                                if (!isStart)
                                {
                                    edge.m_CellStart = cell.m_Position;
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
            aabbBig.m_Min.x = Mathf.FloorToInt(aabbBig.m_Min.x / m_CellSize) * m_CellSize;
            aabbBig.m_Min.y = Mathf.FloorToInt(aabbBig.m_Min.y / m_CellSize) * m_CellSize;
            aabbBig.m_Max.x = Mathf.CeilToInt(aabbBig.m_Max.x / m_CellSize) * m_CellSize;
            aabbBig.m_Max.y = Mathf.CeilToInt(aabbBig.m_Max.y / m_CellSize) * m_CellSize;

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
            Color[] tempColors = new Color[4] {Color.red, Color.green, Color.blue,Color.gray};
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

        public void RemovePanel(LevelPanel panel)
        {
            if (panel == null) return;

            if(m_RoomList.Contains(panel))
            {
                m_RoomList.Remove(panel);
            }

            if (m_DoorList.Contains(panel))
            {
                m_DoorList.Remove(panel);
            }
        }


    }
}

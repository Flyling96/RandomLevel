using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace DragonSlay.RandomLevel
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

        [SerializeField, Header("房间数量")]
        private int m_NeedMainRoom = 5;

        [SerializeField, Header("房间筛选，以宽高之和判定")]
        private int m_RoomFilter = 200;

        [SerializeField, Range(50, 200), Header("随机宽高范围")]
        private int m_PointRandomRange = 120;

        [SerializeField, Range(0, 1f), Header("融合三角剖分百分比")]
        private float m_MixPersents = 0.15f;

        List<LevelMesh> m_LevelMeshList = new List<LevelMesh>();

        public void GenerateMesh()
        {
            m_LevelMeshList.Clear();
            Vector2[] allRect = new Vector2[m_InitCount];
            while (true)
            {
                int main_count = 0;
                for (int i = 0; i < m_InitCount; i++)
                {
                    Vector2 rect;
                    if (main_count == m_NeedMainRoom)
                    {
                        rect = new Vector2(Random.Range(50, m_RoomFilter / 2), Random.Range(50, m_RoomFilter / 2));
                    }
                    else
                    {
                        rect = new Vector2(Random.Range(50, m_PointRandomRange), Random.Range(50, m_PointRandomRange));
                    }
                    if (rect.x + rect.y > m_RoomFilter)
                    {
                        main_count++;
                    }
                    allRect[i] = rect;
                }
                if (main_count >= m_NeedMainRoom)
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
                var meshData = m_LevelMeshList[i];
                var mesh = new Mesh();
                mesh.vertices = meshData.m_Vertices;
                mesh.triangles = meshData.m_Triangles;
                result[i] = mesh;
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

    }
}

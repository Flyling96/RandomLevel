using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    public struct AABoundingBox2D
    {
        public Vector2 m_Min;
        public Vector2 m_Max;

        public AABoundingBox2D(Vector2[] vertices)
        {
            m_Min = new Vector2(float.MaxValue, float.MaxValue);
            m_Max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                if (vertex.x > m_Max.x) m_Max.x = vertex.x;
                if (vertex.x < m_Min.x) m_Min.x = vertex.x;
                if (vertex.y > m_Max.y) m_Max.y = vertex.y;
                if (vertex.y < m_Min.y) m_Min.y = vertex.y;
            }
        }

        public AABoundingBox2D(AABoundingBox2D[] aabbs)
        {
            m_Min = new Vector2(float.MaxValue, float.MaxValue);
            m_Max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < aabbs.Length; i++)
            {
                var aabb = aabbs[i];
                if (aabb.m_Max.x > m_Max.x) m_Max.x = aabb.m_Max.x;
                if (aabb.m_Min.x < m_Min.x) m_Min.x = aabb.m_Min.x;
                if (aabb.m_Max.y > m_Max.y) m_Max.y = aabb.m_Max.y;
                if (aabb.m_Min.y < m_Min.y) m_Min.y = aabb.m_Min.y;
            }
        }

        public static AABoundingBox2D operator+(AABoundingBox2D aabb2D,Vector2 offset)
        {
            aabb2D.m_Min += offset;
            aabb2D.m_Max += offset;
            return aabb2D;
        }
    }

    public struct AABoundingBox
    {
        public Vector3 m_Min;
        public Vector3 m_Max;
    }

    public abstract class LevelMesh2D : LevelMesh
    {
        public Vector3 m_Right = new Vector3(1, 0, 0);

        public Vector3 m_Up = new Vector3(0, 0, 1);

        public Vector2[] m_Borders;

        public virtual AABoundingBox2D GetAABB2D()
        {
            AABoundingBox2D aabb2D = new AABoundingBox2D(m_Borders);
            return aabb2D;
        }

        public AABoundingBox2D GetAABB2D(int voxelSize)
        {
            AABoundingBox2D aabb2D = GetAABB2D();
            aabb2D.m_Min.x = Mathf.FloorToInt(aabb2D.m_Min.x / voxelSize) * voxelSize;
            aabb2D.m_Min.y = Mathf.FloorToInt(aabb2D.m_Min.y / voxelSize) * voxelSize;
            aabb2D.m_Max.x = Mathf.CeilToInt(aabb2D.m_Max.x / voxelSize) * voxelSize;
            aabb2D.m_Max.y = Mathf.CeilToInt(aabb2D.m_Max.y / voxelSize) * voxelSize;
            return aabb2D;
        }

        public override void GenerateVoxel(int voxelSize)
        {
            m_Voxels.Clear();
            AABoundingBox2D aabb2D = GetAABB2D(voxelSize);
            int minX = (int)aabb2D.m_Min.x / voxelSize * voxelSize;
            int minY = (int)aabb2D.m_Min.y / voxelSize * voxelSize;
            int maxX = (int)aabb2D.m_Max.x / voxelSize * voxelSize + voxelSize;
            int maxY = (int)aabb2D.m_Max.y / voxelSize * voxelSize + voxelSize;

            for (int j = minY; j < maxY + 1; j += voxelSize)
            {
                for (int i = minX; i < maxX + 1; i += voxelSize)
                {
                    Vector2 center = new Vector2(i, j);
                    LevelCell levelCell = new LevelCell(center, m_Right, m_Up, voxelSize);
                    if (Mathf.Abs(i) < voxelSize && Mathf.Abs(j) < voxelSize)
                    {
                        m_StartVoxel = levelCell;
                    }
                    m_Voxels.Add(levelCell);
                }
            }
        }

        public override Vector3 CalculateVoxelMeshPos(int voxelSize)
        {
            Vector2 panelPos = new Vector2(Vector3.Dot(m_Position, m_Right), Vector3.Dot(m_Position, m_Up));
            panelPos.x = (int)panelPos.x / voxelSize * voxelSize;
            panelPos.y = (int)panelPos.y / voxelSize * voxelSize;
            Vector3 result = panelPos.x * m_Right + panelPos.y * m_Up;
            return result;
        }

        public virtual Vector2 CalculateVoxelMeshPos2D(int voxelSize)
        {
            Vector2 panelPos = new Vector2(Vector3.Dot(m_Position, m_Right), Vector3.Dot(m_Position, m_Up));
            panelPos.x = (int)panelPos.x / voxelSize * voxelSize;
            panelPos.y = (int)panelPos.y / voxelSize * voxelSize;
            return panelPos;
        }

        public virtual bool IsPointInside(Vector2 point)
        {
            Vector2 p0, p1, v0, v1;
            float cross = 1;
            for (int i = 0; i < m_Borders.Length; i++)
            {
                p0 = m_Borders[i];
                p1 = m_Borders[(i + 1) % m_Borders.Length];
                v0 = p1 - p0;
                v1 = point - p0;
                float temp = v0.x * v1.y - v1.x * v0.y;
                if (i != 0)
                {
                    if (temp * cross <= 0)
                    {
                        return false;
                    }
                }
                cross = temp;

            }
            return true;
        }

    }

    public abstract class LevelMesh
    {
        public Vector3 m_Position;
        public Vector3[] m_Vertices = null;
        public int[] m_Triangles = null;
        //public int m_VoxelSize;

        public List<LevelVoxel> m_Voxels = new List<LevelVoxel>();
        public LevelVoxel m_StartVoxel;

        public void GenerateTriangles()
        {
            int vertexCount = m_Vertices.Length;
            if (vertexCount < 3)
            {
                return;
            }

            m_Triangles = new int[(vertexCount - 1) * 3];

            for(int i =0;i<vertexCount -1;i++)
            {
                m_Triangles[i * 3] = 0;
                m_Triangles[i * 3 + 1] = i + 1;
                m_Triangles[i * 3 + 2] = (i >= vertexCount - 2) ? 1 : i + 2;
            }
        }

        public Mesh ConvertMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = m_Vertices;
            mesh.triangles = m_Triangles;
            return mesh;
        }

        public Mesh ConvertVoxelMesh()
        {
            List<Vector3> vertexList = new List<Vector3>();
            List<int> triangleList = new List<int>();
            List<Color> colorList = new List<Color>();

            Vector3 startPos = m_StartVoxel!= null ? m_StartVoxel.m_Position : Vector3.zero;
            for(int i =0;i< m_Voxels.Count;i++)
            {
                var voxel = m_Voxels[i];
                voxel.FillMesh(vertexList, triangleList, startPos);
                voxel.SetMeshColor(colorList,LevelVoxel.VertexColorType.Show);
            }

            Mesh mesh = new Mesh();
            if(vertexList.Count > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.vertices = vertexList.ToArray();
            mesh.triangles = triangleList.ToArray();
            mesh.colors = colorList.ToArray();

            return mesh;
        }

        public virtual Vector3 CalculateVoxelMeshPos(int voxelSize) { return Vector3.zero; }

        public abstract void GenerateVoxel(int voxelSize);

        public virtual AABoundingBox GetAABB()
        {
            AABoundingBox aabb = new AABoundingBox();
            aabb.m_Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            aabb.m_Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i =0;i< m_Vertices.Length;i++)
            {
                var vertex = m_Vertices[i];
                if (vertex.x > aabb.m_Max.x) aabb.m_Max.x = vertex.x;
                else if (vertex.x < aabb.m_Min.x) aabb.m_Min.x = vertex.x;
                if (vertex.y > aabb.m_Max.y) aabb.m_Max.y = vertex.y;
                else if (vertex.y < aabb.m_Min.y) aabb.m_Min.y = vertex.y;
                if (vertex.z > aabb.m_Max.z) aabb.m_Max.z = vertex.z;
                else if (vertex.z < aabb.m_Min.z) aabb.m_Min.z = vertex.z;
            }
            return aabb;
        }

    }
}

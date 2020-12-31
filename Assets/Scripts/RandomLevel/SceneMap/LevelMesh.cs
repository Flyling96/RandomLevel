using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public struct AABoundingBox2D
    {
        public Vector2 m_Min;
        public Vector2 m_Max;
    }

    public struct AABoundingBox
    {
        public Vector3 m_Min;
        public Vector3 m_Max;
    }

    public abstract class LevelMesh2D : LevelMesh
    {
        protected Vector3 m_Right = new Vector3(1, 0, 0);

        protected Vector3 m_Up = new Vector3(0, 0, 1);

        public Vector2[] m_Borders;

        public virtual AABoundingBox2D GetAABB2D()
        {
            AABoundingBox2D aabb2D = new AABoundingBox2D();
            aabb2D.m_Min = new Vector2(float.MaxValue, float.MaxValue);
            aabb2D.m_Max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < m_Borders.Length; i++)
            {
                var vertex = m_Borders[i];
                if (vertex.x > aabb2D.m_Max.x) aabb2D.m_Max.x = vertex.x;
                else if (vertex.x < aabb2D.m_Min.x) aabb2D.m_Min.x = vertex.x;
                if (vertex.y > aabb2D.m_Max.y) aabb2D.m_Max.y = vertex.y;
                else if (vertex.y < aabb2D.m_Min.y) aabb2D.m_Min.y = vertex.y;
            }
            return aabb2D;
        }

        public override void GenerateVoxel(int voxelSize)
        {
            m_Voxels.Clear();
            AABoundingBox2D aabb2D = GetAABB2D();
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
                    if (IsPointInside(center))
                    {
                        levelCell.m_IsShow = true;
                    }
                    else
                    {
                        levelCell.m_IsShow = false;
                    }
                    m_Voxels.Add(levelCell);
                }
            }
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
                    if (temp * cross < 0)
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

        public List<LevelVoxel> m_Voxels = new List<LevelVoxel>();

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

        public Mesh FillMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = m_Vertices;
            mesh.triangles = m_Triangles;
            return mesh;
        }

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

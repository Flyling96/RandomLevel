using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{

    public abstract class LevelMesh
    {
        public Vector3 m_Position;
        public Vector3[] m_Vertices = null;
        public int[] m_Triangles = null;

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

        public void GenerateVoxel()
        {

        }

    }
}

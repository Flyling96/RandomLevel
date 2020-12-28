using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{

    public abstract class LevelMesh
    {
        public Vector3[] m_Vertices = null;
        public int[] m_Triangles = null;

        public void GenerateTriangles()
        {
            int vertexCount = m_Vertices.Length;
            if (vertexCount < 3)
            {
                return;
            }

            m_Triangles = new int[(vertexCount - 2) * 3];

            for(int i = 2; i < vertexCount;i++)
            {
                m_Triangles[(i - 2)] = 0;
                m_Triangles[(i - 1)] = i - 1;
                m_Triangles[i] = i;
            }
        }

        public void GenerateVoxel()
        {

        }

    }
}

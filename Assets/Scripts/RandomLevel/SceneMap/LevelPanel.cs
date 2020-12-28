using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public enum LevelPanelType
    {
        Rect,
        Sphere,
    }

    public abstract class LevelPanel:LevelMesh
    {
        public Vector3 m_Center;

        public Vector3[] m_Borders;

        public abstract LevelPanelType m_VertexType { get; }

        public abstract void RandomVertex(object[] param);

        public abstract void GenerateMesh();

        public void FillMeshData()
        {

            int oldVertexCount = m_Vertices != null ? m_Vertices.Length : 0;

            m_Vertices = new Vector3[m_Borders.Length + 1];
            m_Vertices[0] = m_Center;
            for (int i = 0; i < m_Borders.Length; i++)
            {
                m_Vertices[i + 1] = m_Borders[i];
            }

            if (oldVertexCount != m_Vertices.Length)
            {
                GenerateTriangles();
            }
        }


    }
}

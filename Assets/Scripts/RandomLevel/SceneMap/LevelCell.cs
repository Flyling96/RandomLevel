using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public class LevelCell : LevelVoxel
    {
        Vector3 m_Right = new Vector3(1, 0, 0);

        Vector3 m_Up = new Vector3(0, 0, 1);

        Vector2 m_Center;

        public LevelCell(Vector2 center,Vector3 right,Vector3 up,int size)
        {
            m_Center = center;
            m_Right = right;
            m_Up = up;
            m_Size = size;
            m_Position = m_Center.x * m_Right + m_Center.y * m_Up;
        }

        public override Mesh ConvertMesh()
        {
            RectPanel rectPanel = new RectPanel(m_Size, m_Size, Vector2.zero, m_Position);
            rectPanel.GenerateMesh();
            return rectPanel.ConvertMesh();
        }

        public int subMeshCenterIndex = 0;
        public int subMeshVerticesCount = 0;

        public override void FillMesh(List<Vector3> vertexList, List<int> triangleList,Vector3 startPos)
        {
            //Mesh subMesh = ConvertMesh();
            RectPanel rectPanel = new RectPanel(m_Size, m_Size, Vector2.zero, m_Position);
            rectPanel.GenerateMesh();
            var subVertices = rectPanel.m_Vertices;
            var subTriangles = rectPanel.m_Triangles;
            subMeshVerticesCount = subVertices.Length;
            subMeshCenterIndex = vertexList.Count;
            Vector3 posOffset = m_Position - startPos;
            for(int i =0;i<subVertices.Length;i++)
            {
                Vector3 newVertex = subVertices[i] + posOffset;
                vertexList.Add(newVertex);
            }

            for(int i =0;i<subTriangles.Length;i++)
            {
                int newIndex = subMeshCenterIndex + subTriangles[i];
                triangleList.Add(newIndex);
            }
        }

        public override void SetMeshColor(List<Color> colorList, VertexColorType colorType)
        {
            Color centerColor = GetVertexColor(colorType);
            Color borderColor = Color.black;
            colorList.Add(centerColor);
            for(int i =0;i< subMeshVerticesCount -1;i++)
            {
                colorList.Add(borderColor);
            }
        }

        public List<LevelMesh> InSideLevelMesh()
        {
            List<LevelMesh> levelMeshList = new List<LevelMesh>();
            foreach(var parent in m_ParentSet)
            {
                if(parent is LevelMesh2D parent2D)
                {
                    var panelPos = parent2D.CalculateVoxelMeshPos2D(m_Size);
                    if(parent2D.IsPointInside(m_Center - panelPos))
                    {
                        levelMeshList.Add(parent);
                    }
                }
            }

            return levelMeshList;
        }

        Color GetVertexColor(VertexColorType colorType)
        {
            switch(colorType)
            {
                case VertexColorType.Show:
                    return Color.white;
            }
            return Color.black;
        }
    }
}

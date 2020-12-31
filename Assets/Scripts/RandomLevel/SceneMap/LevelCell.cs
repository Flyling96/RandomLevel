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

        public bool m_IsShow = false;

        public LevelCell(Vector2 center,Vector3 right,Vector3 up,int size)
        {
            m_Center = center;
            m_Right = right;
            m_Up = up;
            m_Size = size;
            m_Position = m_Center.x * m_Right + m_Center.y * m_Up;
        }

        public override Mesh FillMesh()
        {
            RectPanel rectPanel = new RectPanel(m_Size, m_Size, Vector2.zero, m_Position);
            rectPanel.GenerateMesh();
            return rectPanel.FillMesh();
        }
    }
}

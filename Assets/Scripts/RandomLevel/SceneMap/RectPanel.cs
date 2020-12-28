using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public class RectPanel : LevelPanel
    {
        Vector3 m_Right = new Vector3(1, 0, 0);
        Vector3 m_Up = new Vector3(0, 0, 1);
        float m_Width = 1;
        float m_Height = 1;

        public override LevelPanelType m_VertexType => LevelPanelType.Rect;

        public override void RandomVertex(object[] param)
        {
            m_Width = (float)param[0];
            m_Height = (float)param[1];
        }

        public override void GenerateMesh()
        {
            m_Borders = new Vector3[4];
            float halfWidth = m_Width / 2;
            float halfHeight = m_Height / 2;
            m_Borders[0] = m_Center - halfWidth * m_Right + halfWidth * m_Up;
            m_Borders[1] = m_Center + halfWidth * m_Right + halfWidth * m_Up;
            m_Borders[2] = m_Center + halfWidth * m_Right - halfWidth * m_Up;
            m_Borders[3] = m_Center - halfWidth * m_Right - halfWidth * m_Up;
        }

    }
}

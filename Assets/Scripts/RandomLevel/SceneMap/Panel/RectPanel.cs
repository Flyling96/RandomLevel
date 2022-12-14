using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    public class RectPanel : LevelPanel
    {
        float m_Width = 1;
        float m_Height = 1;

        public override LevelPanelType m_VertexType => LevelPanelType.Rect;

        public RectPanel(float width, float height, Vector2 center, Vector3 pos, float rotateAngle = 0.0f)
        {
            m_Width = width;
            m_Height = height;
            m_Center = center;
            m_Position = pos;
            m_Acreage = m_Width * m_Height;
            m_RotateAngle = rotateAngle;
        }

        public override void GenerateMesh()
        {
            m_Borders = new Vector2[4];
            float halfWidth = m_Width / 2;
            float halfHeight = m_Height / 2;
            m_Borders[0] = m_Center + new Vector2(-halfWidth, halfHeight);
            m_Borders[1] = m_Center + new Vector2(halfWidth, halfHeight);
            m_Borders[2] = m_Center + new Vector2(halfWidth, -halfHeight);
            m_Borders[3] = m_Center + new Vector2(-halfWidth, -halfHeight);

            RotateBorders();
            FillMeshData();
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    public class CirclePanel : LevelPanel
    {
        public float m_Radius = 1;

        public override LevelPanelType m_VertexType =>  LevelPanelType.Circle;

        public CirclePanel(float radius, Vector2 center, Vector3 pos)
        {
            m_Radius = radius;
            m_Center = center;
            m_Position = pos;
            m_Acreage = Mathf.PI * radius * radius;
        }

        public override Shape Shape
        { 
            get
            {
                if (m_Shape == null)
                {
                    Vector2 pos = new Vector2(Vector3.Dot(m_Position, m_Right), Vector3.Dot(m_Position, m_Up)); ;
                    Vector3 normal = Vector3.Cross(m_Right, m_Up); ;
                    m_Shape = new Circle(pos, m_Radius, 20);
                }
                else
                {
                    m_Shape.m_Position = new Vector2(Vector3.Dot(m_Position, m_Right), Vector3.Dot(m_Position, m_Up));
                }
                return m_Shape;
            }
        }

        public override void GenerateMesh()
        {
            int bordersCount = 60;
            m_Borders = new Vector2[bordersCount];
            for(int i =0;i< bordersCount; i++)
            {
                float angle = -i * (360.0f / bordersCount);
                angle = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                m_Borders[i] = dir * m_Radius;
            }
            FillMeshData();
        }

        public override AABoundingBox2D GetAABB2D()
        {
            Vector2[] bounds = new Vector2[4] { new Vector2(-m_Radius, -m_Radius),
            new Vector2(-m_Radius,m_Radius),new Vector2(m_Radius,m_Radius),new Vector2(m_Radius,-m_Radius)};

            AABoundingBox2D res = new AABoundingBox2D(bounds);
            return res;
        }

        public override bool IsPointInside(Vector2 point)
        {
            return Vector2.Distance(point, m_Center) < m_Radius; 
        }
    }
}

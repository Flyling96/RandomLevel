﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    public enum LevelPanelType
    {
        Rect,
        Sphere,
    }


    public abstract class LevelPanel:LevelMesh2D
    {
        public Vector2 m_PanelPosition;

        public Vector2 m_Center;

        public float m_Acreage;

        public abstract LevelPanelType m_VertexType { get; }

        public abstract void GenerateMesh();


        public void FillMeshData()
        {

            int oldVertexCount = m_Vertices != null ? m_Vertices.Length : 0;

            m_Vertices = new Vector3[m_Borders.Length + 1];
            m_Vertices[0] = m_Center.x * m_Right + m_Center.y * m_Up;
            for (int i = 0; i < m_Borders.Length; i++)
            {
                m_Vertices[i + 1] = m_Borders[i].x * m_Right + m_Borders[i].y * m_Up;
            }

            if (oldVertexCount != m_Vertices.Length)
            {
                GenerateTriangles();
            }
        }

        public virtual Polygon ToPolygon()
        {
            Vector2 pos = new Vector2(Vector3.Dot(m_Position, m_Right), Vector3.Dot(m_Position, m_Up)); ;
            Vector3 normal = Vector3.Cross(m_Right, m_Up); ;
            Polygon polygon = new Polygon(m_Center, m_Borders, pos,normal);
            return polygon;
        }

        public virtual void SetPosition(Vector2 panelPosition)
        {
            m_Position = panelPosition.x * m_Right + panelPosition.y * m_Up;
            m_PanelPosition = panelPosition;
        }



    }
}

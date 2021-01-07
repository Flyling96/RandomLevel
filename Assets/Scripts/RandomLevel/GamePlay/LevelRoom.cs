using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace DragonSlay.RandomLevel.Gameplay
{
    public partial class LevelRoom : LevelArea
    {
        public List<LevelMesh> m_MeshList = null;

        public bool m_IsEnd = false;

        public bool m_IsStart = false;

        public int m_Id = -1;

        public int m_Index = -1;

        public List<LevelRoom> m_NeighborList = new List<LevelRoom>();

        public LevelRoom(int id)
        {
            m_Id = id;
            m_MeshList = new List<LevelMesh>();
        }

    }

#if UNITY_EDITOR
    public partial class LevelRoom
    {
        public void DrawGizmos()
        {
            for(int i =0;i< m_MeshList.Count;i++)
            {
                var mesh = m_MeshList[i];
                if(mesh == null)
                {
                    continue;
                }
                Handles.BeginGUI();
                var labelPos = HandleUtility.WorldToGUIPoint(mesh.m_Position);
                GUIStyle style = new GUIStyle();
                if(m_IsStart)
                {
                    style.normal.textColor = Color.green;
                }
                else if(m_IsEnd)
                {
                    style.normal.textColor = Color.blue;
                }
                else
                {
                    style.normal.textColor = Color.yellow;
                }
                style.alignment = TextAnchor.MiddleCenter;
                style.richText = true;
                style.fontSize = 24;
                GUI.Label(new Rect(labelPos, new Vector2(30, 30)), 
                    new GUIContent(string.Format("{0}_{1}",m_Id,m_Index)), style);
                Handles.EndGUI();
            }
        }
#endif
    }
}

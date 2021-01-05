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
        List<LevelMesh> m_RoomMesh = null;

        public bool m_IsEnd = false;

        public bool m_IsStart = false;

        public int m_Id = -1;

        public int m_Index = -1;

        public List<LevelRoom> m_NeighborList = new List<LevelRoom>();

        public LevelRoom(int id, List<LevelMesh> roomMeshList)
        {
            m_Id = id;
            m_RoomMesh = roomMeshList;

        }

    }

#if UNITY_EDITOR
    public partial class LevelRoom
    {
        public void DrawGizmos()
        {
            for(int i =0;i< m_RoomMesh.Count;i++)
            {
                var mesh = m_RoomMesh[i];
                if(mesh == null)
                {
                    continue;
                }
                Handles.BeginGUI();
                var labelPos = HandleUtility.WorldToGUIPoint(mesh.m_Position);
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.alignment = TextAnchor.MiddleCenter;
                style.richText = true;
                style.fontSize = 24;
                GUI.Label(new Rect(labelPos, new Vector2(30, 30)), new GUIContent(m_Id.ToString()), style);
                Handles.EndGUI();
            }
        }
#endif
    }
}

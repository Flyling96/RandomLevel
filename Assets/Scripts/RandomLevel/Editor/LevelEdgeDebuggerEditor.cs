using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DragonSlay.RandomLevel.Scene.Editor
{
    [CustomEditor(typeof(LevelEdgeDebugger))]
    public class LevelEdgeDebuggerEditor : UnityEditor.Editor
    {
        int selectIndex = -1;
        private void OnSceneGUI()
        {
            var debugger = target as LevelEdgeDebugger;
            var edge = debugger.m_Data;
            if(edge == null)
            {
                return;
            }

            for(int i =0;i< edge.m_MidPoints.Length;i++)
            {
                var midPoint = edge.m_MidPoints[i];
                var pos = edge.m_Position + midPoint.x * edge.m_Right + midPoint.y * edge.m_Up;
                DrawSelectionHandle(i, pos);
                if(selectIndex == i)
                {
                    DrawPositionControl(i,edge);
                }
            }

        }



        public void DrawSelectionHandle(int i, Vector3 pos)
        {
            if (Event.current.button != 1)
            {
                float size = HandleUtility.GetHandleSize(pos) * 0.2f;
                Handles.color = Color.white;
                if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap)
                    && selectIndex != i)
                {
                    selectIndex = i;
                }
                // Label it
                Handles.BeginGUI();
                Vector2 labelSize = new Vector2(
                        EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight);
                Vector3 labelPos = HandleUtility.WorldToGUIPointWithDepth(pos);
                if (labelPos.z > 0)
                {
                    labelPos.y -= labelSize.y / 2;
                    labelPos.x -= labelSize.x / 2;
                    GUILayout.BeginArea(new Rect(labelPos, labelSize));
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.black;
                    style.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label(new GUIContent(i.ToString(), "mid " + i), style);
                    GUILayout.EndArea();
                }
                Handles.EndGUI();
            }
        }

        void DrawPositionControl(int i, LevelEdge edge)
        {
            var midPoint = edge.m_MidPoints[i];
            Vector3 pos = edge.m_Position + midPoint.x * edge.m_Right + midPoint.y * edge.m_Up;
            EditorGUI.BeginChangeCheck();
            float size = HandleUtility.GetHandleSize(pos) * 0.1f;
            Handles.SphereHandleCap(0, pos, Quaternion.identity, size, EventType.Repaint);
            pos = Handles.DoPositionHandle(pos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                var debugger = target as LevelEdgeDebugger;
                pos = pos - edge.m_Position;
                var midPos = new Vector2(Vector3.Dot(pos, edge.m_Right), Vector3.Dot(pos, edge.m_Up));
                edge.m_MidPoints[i] = midPos;
                debugger.RefreshMesh();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var debugger = target as LevelEdgeDebugger;
            if (GUILayout.Button("RefreshMesh"))
            {
                debugger.RefreshMesh();
            }
        }

    }

}

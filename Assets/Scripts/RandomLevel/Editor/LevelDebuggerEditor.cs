using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DragonSlay.RandomLevel.Scene
{
    [CustomEditor(typeof(LevelDebugger))]
    public class LevelDebuggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var debugger = target as LevelDebugger;
            if (GUILayout.Button("Clear"))
            {
                debugger.Clear();
            }

            if (GUILayout.Button("GenerateMesh"))
            {
                debugger.GenerateAllPanel();
            }
            if (GUILayout.Button("CollsionSimulate"))
            {
                debugger.CollsionSimulate(500);
            }
            if (GUILayout.Button("FilterMinor"))
            {
                debugger.FilterMinorPanel();
            }
            if(GUILayout.Button("GenerateEdge"))
            {
                debugger.GenerateEdge();
            }

            if (GUILayout.Button("GenerateVoxel"))
            {
                debugger.GenerateVoxel();
            }

            if (GUILayout.Button("GenerateGameplayLevel"))
            {
                debugger.GenerateGameplayLevel();
            }
            //if (GUILayout.Button("EdgeMeshTest"))
            //{
            //    debugger.EdgeMeshTest();
            //}
            if (GUILayout.Button("RefreshColor"))
            {
                debugger.RefreshColor();
            }
            if (GUILayout.Button("TempRefreshColor"))
            {
                debugger.TempRefreshColor();
            }
        }

        private void OnSceneGUI()
        {
            var debugger = target as LevelDebugger;
            if(debugger.m_GameplayLevel != null)
            {
                debugger.m_GameplayLevel.DrawGizmos();
            }
        }
    }
}

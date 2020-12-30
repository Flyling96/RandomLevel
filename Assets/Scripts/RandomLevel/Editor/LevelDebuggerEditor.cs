using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DragonSlay.RandomLevel
{
    [CustomEditor(typeof(LevelDebugger))]
    public class LevelDebuggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var debugger = target as LevelDebugger;
            if(GUILayout.Button("GenerateMesh"))
            {
                debugger.GenerateAllPanel();
            }
            if (GUILayout.Button("CollsionSimulate"))
            {
                debugger.CollsionSimulate(100);
            }
            if (GUILayout.Button("FilterMinor"))
            {
                debugger.FilterMinorPanel();
            }
            if(GUILayout.Button("GenerateEdge"))
            {
                debugger.GenerateEdge();
            }
            if (GUILayout.Button("Clear"))
            {
                debugger.Clear();
            }
            if (GUILayout.Button("EdgeMeshTest"))
            {
                debugger.EdgeMeshTest();
            }
        }
    }
}

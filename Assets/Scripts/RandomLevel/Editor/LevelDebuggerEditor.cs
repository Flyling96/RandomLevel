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
                debugger.GenerateMesh();
            }
        }
    }
}

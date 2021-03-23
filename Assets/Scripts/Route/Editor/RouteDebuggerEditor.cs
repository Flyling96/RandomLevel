using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DragonSlay.Route
{
    [CustomEditor(typeof(RouteDebugger))]
    public class RouteDebuggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var debugger = target as RouteDebugger;
            //if(GUILayout.Button("Start"))
            //{

            //}


            //if(GUILayout.Button("End"))
            //{

            //}

            if (GUILayout.Button("ConvertMesh"))
            {
                debugger.ConvertMesh();
            }
        }

        //void SetRoutePointList(SerializedProperty routeProperty)
        //{

        //}

    }
}

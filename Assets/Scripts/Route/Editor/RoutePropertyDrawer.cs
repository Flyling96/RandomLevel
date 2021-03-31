using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using UnityEngine.UIElements;

namespace DragonSlay.Route
{
    [CustomPropertyDrawer(typeof(Route))]
    public class RoutePropertyDrawer : PropertyDrawer
    {
        GameObject selectTarget = null;
        Route target = null;
        static bool routePointsExpanded;
        int selectIndex = -1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            selectTarget = (property.serializedObject.targetObject as MonoBehaviour).gameObject;
            //base.OnGUI(position, property, label);
            if (target == null)
            {
                target = SerializationHelper.GetTargetObjectOfProperty(property) as Route;
            }

            if (routePointList == null)
            {
                SetRoutePointList(property);
            }

            routePointsExpanded = EditorGUILayout.Foldout(routePointsExpanded, "Route Points", true);
            SceneView.duringSceneGui -= DrawSceneGUI;
            SceneView.duringSceneGui += DrawSceneGUI;
            if (routePointsExpanded)
            {
                routePointList.DoLayoutList();
            }

        }


        private ReorderableList routePointList;
        
        private void SetRoutePointList(SerializedProperty routeProperty)
        {
            routePointList = new ReorderableList(
                routeProperty.serializedObject, routeProperty.FindPropertyRelative("m_Points"),
                true,true,true,true);

            routePointList.elementHeight *= 1;

            routePointList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "RoutePoints");
            };

            routePointList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    DrawRoutePointEditor(rect, index);
                };

            routePointList.onAddCallback = (ReorderableList l) =>
             {
                 InsertRoutePointAtIndex(l.index);
             };

        }

        private void DrawRoutePointEditor(Rect rect, int index)
        {
            Vector2 numberDimension = GUI.skin.button.CalcSize(new GUIContent("999"));
            Vector2 labelDimension = GUI.skin.label.CalcSize(new GUIContent("Position"));
            Vector2 addButtonDimension = new Vector2(labelDimension.y + 5, labelDimension.y + 1);
            float vSpace = 2;
            float hSpace = 3;

            SerializedProperty element = routePointList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += vSpace / 2;

            Rect r = new Rect(rect.position, numberDimension);
            Color color = GUI.color;
            if (GUI.Button(r, new GUIContent(index.ToString(), "Go to the waypoint in the scene view")))
            {
                if (SceneView.lastActiveSceneView != null)
                {
                    routePointList.index = index;
                    SceneView.lastActiveSceneView.pivot = target.TRS.MultiplyPoint(target.m_Points[index].m_LocalPos);
                    SceneView.lastActiveSceneView.size = 3;
                    SceneView.lastActiveSceneView.Repaint();
                }
            }

            GUI.color = color;
            r = new Rect(rect.position, labelDimension);
            r.x += hSpace + numberDimension.x;
            EditorGUI.LabelField(r, "Position");
            r.x += hSpace + r.width;
            r.width = rect.width - (numberDimension.x + hSpace + r.width + hSpace + addButtonDimension.x + hSpace);
            EditorGUI.PropertyField(r, element.FindPropertyRelative("m_LocalPos"), GUIContent.none);

        }

        private void InsertRoutePointAtIndex(int indexA)
        {
            Vector3 pos = Vector3.forward;

            // Get new values from the current indexA (if any)
            int numWaypoints = target.m_Points.Count;
            if (indexA < 0)
                indexA = numWaypoints - 1;
            if (indexA >= 0)
            {
                int indexB = indexA + 1;
                int indexC = indexA - 1;
                var dir = indexC > -1 ? (target.m_Points[indexA].m_LocalPos - target.m_Points[indexC].m_LocalPos).normalized * 5 : Vector3.forward;
                if (indexB >= numWaypoints)
                {
                    // Extrapolate the end
                    pos = target.m_Points[indexA].m_LocalPos + dir;
                }
                else
                {
                    // Interpolate
                    pos = Vector3.Lerp(target.m_Points[indexA].m_LocalPos, target.m_Points[indexB].m_LocalPos, 0.5f);
                }

            }
            Undo.RecordObject(selectTarget, "Add waypoint");
            var point = new RoutePoint(pos);
            point.m_IsMain = true;
            var list = new List<RoutePoint>(target.m_Points);
            list.Insert(indexA + 1, point);
            target.m_Points = list;
            routePointList.index = indexA + 1; // select it
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0 ;
        }

        private void DrawSceneGUI(SceneView obj)
        {
            if (routePointList == null)
                return;

            if (Tools.current == Tool.Move)
            {
                Color colorOld = Handles.color;
                var localToWorld = target.TRS;
                //var localRotation = Target.transform.rotation;
                for (int i = 0; i < target.m_Points.Count; i++)
                {
                    DrawSelectionHandle(i, localToWorld);
                    if (routePointList.index == i)
                    {
                        // Waypoint is selected
                        DrawPositionControl(i, localToWorld);
                    }
                }
                Handles.color = colorOld;

                if (!Event.current.shift && selectIndex!= -1)
                {
                    selectIndex = -1;
                }
            }

            if(Selection.activeObject != selectTarget)
            {
                SceneView.duringSceneGui -= DrawSceneGUI;
            }


        }

        public void DrawSelectionHandle(int i, Matrix4x4 localToWorld)
        {
            Vector3 pos = localToWorld.MultiplyPoint(target.m_Points[i].m_LocalPos);
            float size = HandleUtility.GetHandleSize(pos) * 0.2f;
            Color color = target.m_Points[i].m_IsMain ? Color.green : Color.white;
            Handles.color = color;
            if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap))
            {
                if (Event.current.button == 0)
                {
                    if (Event.current.shift && selectIndex != i)
                    {
                        if (selectIndex == -1)
                        {
                            if (target.m_Points[i].m_IsMain && !target.m_Points[i].IsTurn)
                            {
                                selectIndex = i;
                            }
                        }
                        else
                        {
                            if (target.m_Points[i].m_IsMain && !target.m_Points[i].IsTurn)
                            {
                                target.ConnectPoint(selectIndex, i);
                                selectIndex = -1;
                            }
                        }
                    }
                    else if (routePointList.index != i)
                    {
                        routePointList.index = i;
#if UNITY_2019_1_OR_NEWER
                        if (selectTarget != null)
                            EditorUtility.SetDirty(selectTarget);
#endif
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }
            }
            // Label it
            Handles.BeginGUI();
            Vector2 labelSize = new Vector2(
                    EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight);
            Vector2 labelPos = HandleUtility.WorldToGUIPoint(pos);
            labelPos.y -= labelSize.y / 2;
            labelPos.x -= labelSize.x / 2;
            GUILayout.BeginArea(new Rect(labelPos, labelSize));
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(new GUIContent(i.ToString(), "Waypoint " + i), style);
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        void DrawPositionControl(int i, Matrix4x4 localToWorld)
        {
            RoutePoint point = target.m_Points[i];
            Vector3 pos = localToWorld.MultiplyPoint(point.m_LocalPos);
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.green;
            Quaternion rotation = Quaternion.identity;
            float size = HandleUtility.GetHandleSize(pos) * 0.1f;
            Handles.SphereHandleCap(0, pos, rotation, size, EventType.Repaint);
            pos = Handles.PositionHandle(pos, rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectTarget, "Move Waypoint");
                point.m_LocalPos = Matrix4x4.Inverse(localToWorld).MultiplyPoint(pos); ;
                target.m_Points[i] = point;
#if UNITY_2019_1_OR_NEWER
                if (selectTarget != null)
                    EditorUtility.SetDirty(selectTarget);
#endif
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }


        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy
            | GizmoType.InSelectionHierarchy | GizmoType.Pickable, typeof(RouteDebugger))]
        public static void DrawGizmos(RouteDebugger debugger, GizmoType selectionType)
        {
            var route = debugger.m_Route;
            Color colorOld = Gizmos.color;
            Gizmos.color = Color.green;
            for (int i =0;i < route.m_Points.Count;i++)
            {
                var point = route.m_Points[i];
                Vector3 p0, p1;
                p0 = route.TRS.MultiplyPoint(point.m_LocalPos);
                if(point.m_PrePoint != null)
                {
                    p1 = route.TRS.MultiplyPoint(point.m_PrePoint.m_LocalPos);
                    Gizmos.DrawLine(p0, p1);
                }

                if (point.m_ProPoint != null)
                {
                    p1 = route.TRS.MultiplyPoint(point.m_ProPoint.m_LocalPos);
                    Gizmos.DrawLine(p0, p1);
                }

                for(int k =0; k < point.m_ForkPoints.Count;k++)
                {
                    var forkPoint = point.m_ForkPoints[k];
                    p1 = route.TRS.MultiplyPoint(forkPoint.m_LocalPos);
                    Gizmos.DrawLine(p0, p1);
                }
            }


            Gizmos.color = colorOld;
        }
    }
}

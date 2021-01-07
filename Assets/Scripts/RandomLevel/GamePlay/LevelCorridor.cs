using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonSlay.RandomLevel.Gameplay
{
    public partial class LevelCorridor : LevelArea
    {
        public Dictionary<LevelRoom, LevelCell> m_ConnectRoomCellDic = new Dictionary<LevelRoom, LevelCell>();

        public List<Graph.Edge> GenerateEdge(LevelGraph sceneGraph)
        {
            List<Graph.Edge> edgeList = new List<Graph.Edge>();
            List<LevelRoom> roomList = new List<LevelRoom>();
            foreach(var key in m_ConnectRoomCellDic.Keys)
            {
                roomList.Add(key);
            }

            for(int i =0;i< roomList.Count;i++)
            {
                var startRoom = roomList[i];
                LevelCell startCell;
                if(!m_ConnectRoomCellDic.TryGetValue(startRoom, out startCell))
                {
                    continue;
                }

                for(int j = i + 1; j < roomList.Count;j++)
                {
                    var targetRoom = roomList[j];
                    int distance = CalculateDistance(startCell, targetRoom, sceneGraph);
                    if (distance > 0)
                    {
                        var edge = new Graph.Edge();
                        edge.m_Point0 = startRoom.m_Id;
                        edge.m_Point1 = targetRoom.m_Id;
                        edge.m_Distance = distance;
                        edgeList.Add(edge);
                    }
                }
            }

            return edgeList;
        }

        public int CalculateDistance(LevelCell start,LevelRoom target, LevelGraph sceneGraph)
        {
            var cellDic = sceneGraph.m_LevelCellDic;
            var cellSize = sceneGraph.m_CellSize;
            int distance = 0;

            var offsets = new Vector2[4] { new Vector2(0, cellSize), new Vector2(0, -cellSize),
                new Vector2(-cellSize, 0), new Vector2(cellSize, 0) };

            Vector2 nextPos;
            LevelCell nextCell;

            HashSet<Vector2> findNext = new HashSet<Vector2>();
            HashSet<Vector2> alreadySet = new HashSet<Vector2>();
            findNext.Add(start.m_Center);
            alreadySet.Add(start.m_Center);
            bool isBreak = false;
            while (findNext.Count != 0)
            {
                HashSet<Vector2> nextSet = new HashSet<Vector2>();
                foreach (var pos in findNext)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        nextPos = pos + offsets[j];
                        if (cellDic.TryGetValue(nextPos, out nextCell))
                        {
                            if (nextCell.m_SceneCell.IsMaskCell(SceneCellType.Corridor) && !nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room)
                                && !alreadySet.Contains(nextPos))
                            {
                                nextSet.Add(nextPos);
                            }
                            else if (nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room))
                            {
                                var room = nextCell.GameplayBelong as LevelRoom;
                                if (room != null)
                                {
                                    if(room == target)
                                    {
                                        isBreak = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                findNext = nextSet;
                distance++;
                if(isBreak)
                {
                    break;
                }

            }

            return distance;
        }
    }

#if UNITY_EDITOR
    public partial class LevelCorridor
    {
        public void DrawGizmos()
        {
            string idStr = "";
            foreach(var room in m_ConnectRoomCellDic.Keys)
            {
                idStr = string.Format("{0}_{1}", idStr, room.m_Id);
            }

            Handles.BeginGUI();
            var labelPos = HandleUtility.WorldToGUIPoint(m_Position);
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;
            style.richText = true;
            style.fontSize = 24;
            GUI.Label(new Rect(labelPos, new Vector2(30, 30)), new GUIContent(idStr), style);
            Handles.EndGUI();
        }
#endif
    }
}

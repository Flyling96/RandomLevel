using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;
using System;
using Random = UnityEngine.Random;
using System.Linq;

namespace DragonSlay.RandomLevel.Gameplay
{
    public partial class Level
    {

        [SerializeField, Header("起点终点最小深度")]
        private int m_MinDepth = 6;

        Dictionary<Vector3, LevelCell> m_CellPosDic = new Dictionary<Vector3, LevelCell>();
        List<LevelRoom> m_LevelRoomList = new List<LevelRoom>();
        List<LevelCorridor> m_LevelCorridorList = new List<LevelCorridor>();
        List<Door> m_DoorList = new List<Door>();
        Graph m_Graph = null;

        public void Init(LevelGraph sceneGraph)
        {
            m_LevelRoomList?.Clear();
            m_LevelCorridorList?.Clear();
            m_DoorList?.Clear();
            m_CellPosDic = sceneGraph.m_LevelCellDic;

            foreach (var cell in sceneGraph.m_LevelCellDic.Values)
            {
                cell.m_GameplayCell = new GameplayCell();
            }

            float count = 6;
            int index = 0;
#if UNITY_EDITOR
            index++;
            UnityEditor.EditorUtility.DisplayProgressBar("InitLevel", "Init Room", index / count);
#endif
            InitRoom(sceneGraph);
#if UNITY_EDITOR
            index++;
            UnityEditor.EditorUtility.DisplayProgressBar("InitLevel", "Init Corridor", index / count);
#endif
            InitCorridor(sceneGraph);
#if UNITY_EDITOR
            index++;
            UnityEditor.EditorUtility.DisplayProgressBar("InitLevel", "Init Door", index / count);
#endif
            InitDoor(sceneGraph);
#if UNITY_EDITOR
            index++;
            UnityEditor.EditorUtility.DisplayProgressBar("InitLevel", "Init Wall", index / count);
#endif
            InitWall(sceneGraph);
#if UNITY_EDITOR
            index++;
            UnityEditor.EditorUtility.DisplayProgressBar("InitLevel", "Init Graph", index / count);
#endif
            InitGraph(sceneGraph);
#if UNITY_EDITOR
            index++;
            UnityEditor.EditorUtility.DisplayProgressBar("InitLevel", "Init StartEnd", index / count);
#endif
            InitStartEnd();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }

        void InitRoom(LevelGraph sceneGraph)
        {
            var roomMeshList = sceneGraph.m_RoomList;
            var cellDic = sceneGraph.m_LevelCellDic;
            var cellSize = sceneGraph.m_CellSize;

            var offsets = new Vector2[4] { new Vector2(0, cellSize), new Vector2(0, -cellSize), 
                new Vector2(-cellSize, 0), new Vector2(cellSize, 0) };
            int index = 0;

            Vector3 nextPos;
            LevelCell nextCell;
            for (int i =0;i< roomMeshList.Count;i++)
            {
                var roomMesh = roomMeshList[i];
                LevelCell cell = null;
                if(!cellDic.TryGetValue(roomMesh.m_CellStart,out cell))
                {
                    continue;
                }

                if(cell.GameplayBelong != null && cell.GameplayBelong is LevelRoom belongRoom)
                {
                    if(!belongRoom.m_MeshList.Contains(roomMesh))
                    {
                        belongRoom.m_MeshList.Add(roomMesh);
                    }
                    continue;
                }

                Vector3 right = roomMesh.m_Right;
                Vector3 up = roomMesh.m_Up;
                LevelRoom room = new LevelRoom(index);
                room.m_Right = right;
                room.m_Up = up;
                m_LevelRoomList.Add(room);
                index++;

                cell.GameplayBelong = room;
                room.m_MeshList.Add(roomMesh);
                room.m_CenterCell = cell;

                HashSet<Vector3> findNext = new HashSet<Vector3>();
                findNext.Add(roomMesh.m_CellStart);
                while (findNext.Count != 0)
                {
                    HashSet<Vector3> nextSet = new HashSet<Vector3>();
                    foreach (var pos in findNext)
                    {
                        for(int j = 0;j < 4; j++)
                        {
                            nextPos = pos + offsets[j].x * right + offsets[j].y * up;
                            if (cellDic.TryGetValue(nextPos, out nextCell))
                            {
                                if (nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room)&& nextCell.GameplayBelong == null)
                                {
                                    nextCell.GameplayBelong = room;
                                    if (!nextCell.m_SceneCell.IsMaskCell(SceneCellType.Door))
                                    {
                                        nextCell.m_SceneCell.OnlyMaskCell(SceneCellType.Room);
                                    }
                                    nextSet.Add(nextPos);
                                }
                                else if (nextCell.m_SceneCell.IsNull)
                                {
                                    nextCell.m_SceneCell.OnlyMaskCell(SceneCellType.Wall);
                                }
                            }
                        }
                    }
                    findNext = nextSet;
                }
            }
        }

        void InitCorridor(LevelGraph sceneGraph)
        {
            var edgeList = sceneGraph.m_EdgeList;
            var cellDic = sceneGraph.m_LevelCellDic;
            var cellSize = sceneGraph.m_CellSize;
            var offsets = new Vector2[4] { new Vector2(0, cellSize), new Vector2(0, -cellSize),
                new Vector2(-cellSize, 0), new Vector2(cellSize, 0) };
            //int index = 0;

            Vector3 nextPos;
            LevelCell nextCell;

            foreach (var edge in edgeList)
            {

                Vector3 right = edge.m_Right;
                Vector3 up = edge.m_Up;

                LevelCorridor corridor = new LevelCorridor();
                corridor.m_Right = right;
                corridor.m_Up = up;

                if (!cellDic.TryGetValue(edge.m_CellStart, out nextCell))
                {
                    continue;
                }

                nextCell.GameplayBelong = corridor;
                corridor.m_CenterCell = nextCell;

                corridor.m_Position = edge.m_CellStart;
                HashSet<Vector3> findNext = new HashSet<Vector3>();
                findNext.Add(edge.m_CellStart);

                while (findNext.Count != 0)
                {
                    HashSet<Vector3> nextSet = new HashSet<Vector3>();
                    foreach (var pos in findNext)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            nextPos = pos + offsets[j].x * right + offsets[j].y * up;
                            if (cellDic.TryGetValue(nextPos, out nextCell))
                            {
                                if ((nextCell.m_SceneCell.IsMaskCell(SceneCellType.Corridor) && !nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room)) 
                                    && nextCell.m_GameplayCell.m_Belong == null)
                                {
                                    nextCell.GameplayBelong = corridor;
                                    nextSet.Add(nextPos);
                                }
                                else if (nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room))
                                {
                                    var room = nextCell.GameplayBelong as LevelRoom;
                                    if (room != null)
                                    {
                                        if(!corridor.m_ConnectRoomCellDic.ContainsKey(room))
                                        {
                                            corridor.m_ConnectRoomCellDic.Add(room, nextCell);
                                        }
                                    }
                                }
                                else if(nextCell.m_SceneCell.IsNull)
                                {
                                    nextCell.m_SceneCell.OnlyMaskCell(SceneCellType.Wall);
                                }
                            }
                        }
                    }
                    findNext = nextSet;
                }

                if(corridor.m_ConnectRoomCellDic.Count == 1)
                {
                    var room = corridor.m_ConnectRoomCellDic.First().Key;
                    if (room != null)
                    {
                        foreach (var cell in corridor.m_Cells)
                        {
                            cell.GameplayBelong = room;
                            cell.m_SceneCell.OnlyMaskCell(SceneCellType.Room);
                        }
                    }
                }
                else
                {
                    m_LevelCorridorList.Add(corridor);
                }

            }
        }

        void InitDoor(LevelGraph sceneGraph)
        {
            var doorList = sceneGraph.m_DoorList;
            var cellDic = sceneGraph.m_LevelCellDic;
            var cellSize = sceneGraph.m_CellSize;
            var offsets = new Vector2[4] { new Vector2(0, cellSize), new Vector2(0, -cellSize),
                new Vector2(-cellSize, 0), new Vector2(cellSize, 0) };

            Vector3 nextPos;
            LevelCell nextCell;

            foreach (var doorMesh in doorList)
            {
                LevelCell cell = null;
                if (!cellDic.TryGetValue(doorMesh.m_CellStart, out cell))
                {
                    continue;
                }

                Vector3 right = doorMesh.m_Right;
                Vector3 up = doorMesh.m_Up;
                Door door = new Door();
                m_DoorList.Add(door);

                door.m_Right = right;
                door.m_Up = up;
                door.m_CenterCell = cell;
                door.m_Position = doorMesh.m_CellStart;

                HashSet<Vector3> findNext = new HashSet<Vector3>();
                HashSet<Vector3> alreadyFind = new HashSet<Vector3>();
                findNext.Add(doorMesh.m_CellStart);
                alreadyFind.Add(doorMesh.m_CellStart);

                while (findNext.Count != 0)
                {
                    HashSet<Vector3> nextSet = new HashSet<Vector3>();
                    foreach (var pos in findNext)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            nextPos = pos + offsets[j].x * right + offsets[j].y * up;
                            if (cellDic.TryGetValue(nextPos, out nextCell))
                            {
                                if (nextCell.m_SceneCell.IsMaskCell(SceneCellType.Door)
                                    && !alreadyFind.Contains(nextPos))
                                {
                                    door.m_Cells.Add(nextCell);
                                    nextSet.Add(nextPos);
                                    alreadyFind.Add(nextPos);
                                }
                                else if (nextCell.m_SceneCell.IsNull)
                                {
                                    nextCell.m_SceneCell.OnlyMaskCell(SceneCellType.Wall);
                                }
                            }
                        }
                    }
                    findNext = nextSet;
                }
            }
        }

        void InitWall(LevelGraph sceneGraph)
        {
            var cellDic = sceneGraph.m_LevelCellDic;
            var cellSize = sceneGraph.m_CellSize;

            var offsets = new Vector2[4] { new Vector2(0, cellSize), new Vector2(0, -cellSize),
                new Vector2(-cellSize, 0), new Vector2(cellSize, 0) };
            Vector3 nextPos;
            LevelCell nextCell;

            for (int i = 0; i < m_LevelRoomList.Count; i++)
            {
                var room = m_LevelRoomList[i];
                var start = room.m_CenterCell;
                if(start == null)
                {
                    continue;
                }

                Vector3 right = room.m_Right;
                Vector3 up = room.m_Up;

                HashSet<Vector3> findNext = new HashSet<Vector3>();
                HashSet<Vector3> alreadyFind = new HashSet<Vector3>();
                findNext.Add(start.m_Position);
                alreadyFind.Add(start.m_Position);
                while (findNext.Count != 0)
                {
                    HashSet<Vector3> nextSet = new HashSet<Vector3>();
                    foreach (var pos in findNext)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            nextPos = pos + offsets[j].x * right + offsets[j].y * up;
                            if (cellDic.TryGetValue(nextPos, out nextCell))
                            {
                                if (!nextCell.m_SceneCell.IsMaskCell(SceneCellType.Door))
                                {
                                    if (nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room) && !alreadyFind.Contains(nextPos))
                                    {
                                        alreadyFind.Add(nextPos);
                                        nextSet.Add(nextPos);
                                    }
                                    else if ((nextCell.m_SceneCell.IsMaskCell(SceneCellType.Corridor)))
                                    {
                                        nextCell.m_SceneCell.OnlyMaskCell(SceneCellType.Wall);
                                    }
                                }
                            }
                        }
                    }
                    findNext = nextSet;
                }
            }
        }

        void InitGraph(LevelGraph sceneGraph)
        {
            m_Graph = new Graph();
            for (int i =0;i< m_LevelCorridorList.Count;i++)
            {
                var corridor = m_LevelCorridorList[i];
                var corridorEdges = corridor.GenerateEdge(sceneGraph);
                for(int j =0;j < corridorEdges.Count;j++)
                {
                    var edge = corridorEdges[j];
                    m_Graph.AddEdge(edge);
                }
            }
        }

        public void InitStartEnd()
        {
            if(m_LevelRoomList.Count < m_MinDepth)
            {
                return;
            }

            for (int i = 0; i < m_LevelRoomList.Count; i++)
            {
                m_LevelRoomList[i].m_IsStart = false;
                m_LevelRoomList[i].m_IsEnd = false;
            }

            int maxCount = 100;
            int start = 0;
            List<int> endList = new List<int>();
            while(maxCount -- > 0)
            {
                start = Random.Range(0, m_LevelRoomList.Count);
                var pointDepth = m_Graph.GetPointDepth(start);
                endList.Clear();
                foreach (var keyValue in pointDepth)
                {
                    int id = keyValue.Key;
                    int depth = keyValue.Value;
                    m_LevelRoomList[id].m_Index = depth;
                    if (depth >= m_MinDepth)
                    {
                        endList.Add(id);
                    }
                }
                if(endList.Count > 0)
                {
                    break;
                }
            }

            if (endList.Count > 0)
            {
                int end = endList[Random.Range(0, endList.Count)];
                m_LevelRoomList[start].m_IsStart = true;
                m_LevelRoomList[end].m_IsEnd = true;
            }

        }

        //废弃，考虑从pixel层去组织房间，而不是Panel层
#region 废弃
        //void InitRoom(List<LevelPanel> roomMeshList, List<LevelEdge> edgeMeshList,int voxelSize)
        //{
        //    List<Polygon> polygonList = new List<Polygon>();
        //    SeparatingAxisAlgorithm separatingAxis = new SeparatingAxisAlgorithm();
        //    m_LevelRoomList.Clear();

        //    for (int i = 0; i < roomMeshList.Count; i++)
        //    {
        //        var roomMesh = roomMeshList[i];
        //        AABoundingBox2D aabb2D = roomMesh.GetAABB2D(voxelSize);
        //        aabb2D.m_Min += Vector2.one;
        //        aabb2D.m_Max -= Vector2.one;
        //        Vector2 pos = roomMesh.m_PanelPosition;
        //        Vector2[] borders = new Vector2[4] { aabb2D.m_Min, new Vector2(aabb2D.m_Min.x,aabb2D.m_Max.y),
        //            aabb2D.m_Max,new Vector2(aabb2D.m_Max.x,aabb2D.m_Min.y)};
        //        Polygon polygon = new Polygon(Vector2.zero, borders, pos,  Vector3.up);
        //        polygonList.Add(polygon);
        //    }

        //    int roomId = 0;
        //    for(int j =0;j< roomMeshList.Count;j++)
        //    {
        //        if (roomMeshList[j] == null)
        //        {
        //            continue;
        //        }

        //        List<LevelMesh> levelMeshList = new List<LevelMesh>();
        //        levelMeshList.Add(roomMeshList[j]);
        //        roomMeshList[j] = null;
        //        List<int> findPanelIdList = new List<int>();
        //        findPanelIdList.Add(j);
        //        FillNearPanel(levelMeshList, findPanelIdList, roomMeshList, edgeMeshList, polygonList, voxelSize);
        //        LevelRoom levelRoom = new LevelRoom(roomId,levelMeshList);
        //        m_LevelRoomList.Add(levelRoom);
        //        roomId++;
        //    }
        //}

        //void FillNearPanel(List<LevelMesh> meshList, List<int> findPanelIdList, 
        //    List<LevelPanel> roomMeshList, List<LevelEdge> edgeMeshList, List<Polygon> polygonList,int voxelSize)
        //{
        //    SeparatingAxisAlgorithm separatingAxis = new SeparatingAxisAlgorithm();
        //    List<int> newFindIdList = new List<int>();
        //    for (int j = 0;j < findPanelIdList.Count;j++)
        //    {
        //        int findId = findPanelIdList[j];
        //        List<LevelEdge> connectEdgeList = new List<LevelEdge>();
        //        Polygon polygon0 = polygonList[findId];

        //        for (int i = 0; i < edgeMeshList.Count; i++)
        //        {
        //            var edge = edgeMeshList[i];
        //            if (edge.Data.Point0.Id == findId && edge.Data.Point1.Id < roomMeshList.Count && roomMeshList[edge.Data.Point1.Id] != null)
        //            {
        //                connectEdgeList.Add(edge);
        //            }
        //            else if (edge.Data.Point1.Id == findId && edge.Data.Point0.Id < roomMeshList.Count && roomMeshList[edge.Data.Point0.Id] != null)
        //            {
        //                connectEdgeList.Add(edge);
        //            }
        //        }

        //        for (int i = 0; i < connectEdgeList.Count; i++)
        //        {
        //            var edge = connectEdgeList[i];
        //            int index = -1;
        //            if (edge.Data.Point0.Id != j)
        //            {
        //                index = edge.Data.Point0.Id;
        //            }
        //            else if (edge.Data.Point1.Id != j)
        //            {
        //                index = edge.Data.Point1.Id;
        //            }

        //            if (roomMeshList[index] == null)
        //            {
        //                continue;
        //            }

        //            Polygon polygon1 = polygonList[index];
        //            float distance = separatingAxis.SeparatingAxisDistance(polygon0, polygon1);
        //            if (Mathf.Abs(distance) < 1.1f * voxelSize)
        //            {
        //                newFindIdList.Add(index);
        //                meshList.Add(roomMeshList[index]);
        //                meshList.Add(edge);
        //                roomMeshList[index] = null;
        //            }
        //        }
        //    }

        //    if(newFindIdList.Count > 0)
        //    {
        //        FillNearPanel(meshList, newFindIdList, roomMeshList, edgeMeshList, polygonList, voxelSize);
        //    }
        //}

        //void InitRandomRoom(List<LevelPanel> roomMeshList, List<LevelEdge> edgeMeshList)
        //{
        //    int meshCount = roomMeshList.Count;
        //    int freeCount = meshCount - m_RoomCount;

        //    int[] randomArray = new int[meshCount];
        //    for(int i =0;i< roomMeshList.Count;i++)
        //    {
        //        randomArray[i] = i;
        //    }
        //    int alreadyRandomCount = 0;

        //    for(int j =0;j < m_RoomCount;j++)
        //    {
        //        int meshCountInRoom;
        //        if (j == m_RoomCount - 1)
        //        {
        //             meshCountInRoom = freeCount + 1;
        //        }
        //        else
        //        {
        //             meshCountInRoom = Random.Range(1, freeCount + 2);
        //        }
        //        int allRandomCount = meshCount - 1 - alreadyRandomCount;
        //        int randomIndex = 0;
        //        randomIndex = Random.Range(0, 100 * meshCount) % (allRandomCount + 1);
        //        int centerIndex = randomArray[randomIndex];
        //        randomArray[randomIndex] = randomArray[allRandomCount];
        //        alreadyRandomCount++;
        //        List<LevelEdge> connectEdgeList = new List<LevelEdge>();

        //        for (int i = 0; i < edgeMeshList.Count; i++)
        //        {
        //            var edge = edgeMeshList[i];
        //            if (edge.Data.Point0.Id == centerIndex && edge.Data.Point1.Id < roomMeshList.Count && roomMeshList[edge.Data.Point1.Id] != null)
        //            {
        //                connectEdgeList.Add(edge);
        //            }
        //            else if (edge.Data.Point1.Id == centerIndex && edge.Data.Point0.Id < roomMeshList.Count && roomMeshList[edge.Data.Point0.Id] != null)
        //            {
        //                connectEdgeList.Add(edge);
        //            }
        //        }
        //        meshCountInRoom = Mathf.Min(meshCountInRoom, connectEdgeList.Count + 1);

        //        connectEdgeList.Sort((a, b) =>
        //        {
        //            if (a.Data.Distance > b.Data.Distance)
        //            {
        //                return -1;
        //            }
        //            else
        //            {
        //                return 1;
        //            }
        //        });

        //        List<LevelMesh> meshList = new List<LevelMesh>();
        //        meshList.Add(roomMeshList[centerIndex]);
        //        roomMeshList[centerIndex] = null;
        //        for (int i = 0; i < meshCountInRoom - 1; i++)
        //        {
        //            var edge = connectEdgeList[i];
        //            int index = -1;
        //            if (edge.Data.Point0.Id != centerIndex)
        //            {
        //                index = edge.Data.Point0.Id;
        //            }
        //            else if (edge.Data.Point1.Id != centerIndex)
        //            {
        //                index = edge.Data.Point1.Id;
        //            }

        //            if (index < 0 || index > roomMeshList.Count - 1 && roomMeshList[i] != null)
        //            {
        //                continue;
        //            }

        //            meshList.Add(roomMeshList[index]);
        //            meshList.Add(connectEdgeList[i]);
        //            roomMeshList[index] = null;
        //            allRandomCount--;
        //            for (int k =0;k<allRandomCount + 1;k++)
        //            {
        //                if(randomArray[k] == index)
        //                {
        //                    randomArray[k] = randomArray[allRandomCount];
        //                    break;
        //                }
        //            }

        //            alreadyRandomCount++;
        //        }

        //        freeCount -= meshCountInRoom - 1;

        //        LevelRoom levelRoom = new LevelRoom(j,meshList);
        //        m_LevelRoomList.Add(levelRoom);


        //    }
        //}

        //void InitCorridor(LevelGraph sceneGraph)
        //{

        //}
#endregion

        public void Clear()
        {
            m_LevelRoomList?.Clear();
            m_LevelCorridorList?.Clear();
        }



    }

#if UNITY_EDITOR
    [Serializable]
    public partial class Level
    {
        public void DrawGizmos()
        {
            if (m_LevelRoomList != null)
            {

                for (int i = 0; i < m_LevelRoomList.Count; i++)
                {
                    var room = m_LevelRoomList[i];
                    room.DrawGizmos();
                }
            }

            //if(m_LevelCorridorList != null)
            //{
            //    for(int i =0;i< m_LevelCorridorList.Count;i++)
            //    {
            //        var corridor = m_LevelCorridorList[i];
            //        corridor.DrawGizmos();
            //    }
            //}
        }
    }
#endif
}

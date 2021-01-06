using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;
using System;
using Random = UnityEngine.Random;

namespace DragonSlay.RandomLevel.Gameplay
{
    public partial class Level
    {

        List<LevelRoom> m_LevelRoomList = new List<LevelRoom>();

        public void Init(LevelGraph sceneGraph)
        {
            m_LevelRoomList.Clear();

            //List<LevelPanel> roomMeshList = new List<LevelPanel>();
            //for (int i = 0; i < sceneGraph.m_PanelList.Count; i++)
            //{
            //    roomMeshList.Add(sceneGraph.m_PanelList[i]);
            //}

            //List<LevelEdge> edgeMeshList = new List<LevelEdge>();
            //for (int i = 0; i < sceneGraph.m_EdgeList.Count; i++)
            //{
            //    edgeMeshList.Add(sceneGraph.m_EdgeList[i]);
            //}
            foreach(var cell in sceneGraph.m_LevelCellDic.Values)
            {
                cell.m_GameplayCell = new GameplayCell();
            }

            InitRoom(sceneGraph);
        }

        void InitRoom(LevelGraph sceneGraph)
        {
            var roomMeshList = sceneGraph.m_PanelList;
            var cellDic = sceneGraph.m_LevelCellDic;
            var cellSize = sceneGraph.m_CellSize;

            var offsets = new Vector2[4] { new Vector2(0, cellSize), new Vector2(0, -cellSize), 
                new Vector2(-cellSize, 0), new Vector2(cellSize, 0) };
            int index = 0;

            Vector2 nextPos;
            LevelCell nextCell;
            for (int i =0;i< roomMeshList.Count;i++)
            {
                var roomMesh = roomMeshList[i];
                LevelCell cell = null;
                if(!cellDic.TryGetValue(roomMesh.m_CellStart,out cell))
                {
                    continue;
                }

                if(cell.m_GameplayCell.m_Belong != null)
                {
                    var belong = cell.m_GameplayCell.m_Belong;
                    if(!belong.m_RoomMesh.Contains(roomMesh))
                    {
                        belong.m_RoomMesh.Add(roomMesh);
                    }
                    continue;
                }

                LevelRoom room = new LevelRoom(index);
                m_LevelRoomList.Add(room);
                index++;

                cell.m_GameplayCell.m_Belong = room;
                room.m_RoomMesh.Add(roomMesh);

                HashSet<Vector2> findNext = new HashSet<Vector2>();
                findNext.Add(roomMesh.m_CellStart);
                while (findNext.Count != 0)
                {
                    HashSet<Vector2> nextSet = new HashSet<Vector2>();
                    foreach (var pos in findNext)
                    {
                        for(int j = 0;j < 4; j++)
                        {
                            nextPos = pos + offsets[j];
                            if (cellDic.TryGetValue(nextPos, out nextCell))
                            {
                                if (nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room) && nextCell.m_GameplayCell.m_Belong == null)
                                {
                                    nextCell.m_GameplayCell.m_Belong = room;
                                    nextSet.Add(nextPos);
                                }
                                else if(nextCell.m_SceneCell.IsMaskCell(SceneCellType.Corridor) && !nextCell.m_SceneCell.IsMaskCell(SceneCellType.Room))
                                {
                                    nextCell.m_SceneCell.MaskCell(SceneCellType.Door);

                                }
                            }
                        }
                    }
                    findNext = nextSet;
                }
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
        }



    }

#if UNITY_EDITOR
    [Serializable]
    public partial class Level
    {
        public void DrawGizmos()
        {
            if(m_LevelRoomList == null)
            {
                return;
            }

            for(int i =0;i<m_LevelRoomList.Count;i++)
            {
                var room = m_LevelRoomList[i];
                room.DrawGizmos();
            }
        }
    }
#endif
}

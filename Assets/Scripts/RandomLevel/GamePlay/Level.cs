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
        [SerializeField,Header("房间个数")]
        private int m_RoomCount = 2;

        List<LevelRoom> m_LevelRoomList = new List<LevelRoom>();

        public void Init(LevelGraph sceneGraph)
        {
            m_LevelRoomList.Clear();
            m_RoomCount = Mathf.Min(m_RoomCount, sceneGraph.m_PanelList.Count);

            List<LevelPanel> roomMeshList = new List<LevelPanel>();
            for(int i =0;i<sceneGraph.m_PanelList.Count;i++)
            {
                roomMeshList.Add(sceneGraph.m_PanelList[i]);
            }

            List<LevelEdge> edgeMeshList = new List<LevelEdge>();
            for(int i =0;i< sceneGraph.m_EdgeList.Count;i++)
            {
                edgeMeshList.Add(sceneGraph.m_EdgeList[i]);
            }

            InitRoom(roomMeshList, edgeMeshList);
        }

        void InitRoom(List<LevelPanel> roomMeshList, List<LevelEdge> edgeMeshList)
        {
            int meshCount = roomMeshList.Count;
            int freeCount = meshCount - m_RoomCount;

            int[] randomArray = new int[meshCount];
            for(int i =0;i< roomMeshList.Count;i++)
            {
                randomArray[i] = i;
            }
            int alreadyRandomCount = 0;

            for(int j =0;j < m_RoomCount;j++)
            {
                int meshCountInRoom;
                if (j == m_RoomCount - 1)
                {
                     meshCountInRoom = freeCount + 1;
                }
                else
                {
                     meshCountInRoom = Random.Range(1, freeCount + 2);
                }
                int allRandomCount = meshCount - 1 - alreadyRandomCount;
                int randomIndex = 0;
                randomIndex = Random.Range(0, 100 * meshCount) % (allRandomCount + 1);
                int centerIndex = randomArray[randomIndex];
                randomArray[randomIndex] = randomArray[allRandomCount];
                alreadyRandomCount++;
                List<LevelEdge> connectEdgeList = new List<LevelEdge>();

                for (int i = 0; i < edgeMeshList.Count; i++)
                {
                    var edge = edgeMeshList[i];
                    if (edge.Data.Point0.Id == centerIndex && edge.Data.Point1.Id < roomMeshList.Count && roomMeshList[edge.Data.Point1.Id] != null)
                    {
                        connectEdgeList.Add(edge);
                    }
                    else if (edge.Data.Point1.Id == centerIndex && edge.Data.Point0.Id < roomMeshList.Count && roomMeshList[edge.Data.Point0.Id] != null)
                    {
                        connectEdgeList.Add(edge);
                    }
                }
                meshCountInRoom = Mathf.Min(meshCountInRoom, connectEdgeList.Count + 1);

                connectEdgeList.Sort((a, b) =>
                {
                    if (a.Data.Distance > b.Data.Distance)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });

                List<LevelMesh> meshList = new List<LevelMesh>();
                meshList.Add(roomMeshList[centerIndex]);
                roomMeshList[centerIndex] = null;
                for (int i = 0; i < meshCountInRoom - 1; i++)
                {
                    var edge = connectEdgeList[i];
                    int index = -1;
                    if (edge.Data.Point0.Id != centerIndex)
                    {
                        index = edge.Data.Point0.Id;
                    }
                    else if (edge.Data.Point1.Id != centerIndex)
                    {
                        index = edge.Data.Point1.Id;
                    }

                    if (index < 0 || index > roomMeshList.Count - 1 && roomMeshList[i] != null)
                    {
                        continue;
                    }

                    meshList.Add(roomMeshList[index]);
                    meshList.Add(connectEdgeList[i]);
                    roomMeshList[index] = null;
                    allRandomCount--;
                    for (int k =0;k<allRandomCount + 1;k++)
                    {
                        if(randomArray[k] == index)
                        {
                            randomArray[k] = randomArray[allRandomCount];
                            break;
                        }
                    }

                    alreadyRandomCount++;
                }

                freeCount -= meshCountInRoom - 1;

                LevelRoom levelRoom = new LevelRoom(j,meshList);
                m_LevelRoomList.Add(levelRoom);


            }
        }

        void InitCorridor(LevelGraph sceneGraph)
        {

        }

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

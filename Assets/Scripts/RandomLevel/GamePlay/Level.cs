using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;
using System;
using Random = UnityEngine.Random;

namespace DragonSlay.RandomLevel.Level
{
    [Serializable]
    public class Level
    {
        [SerializeField, Header("房间个数")]
        private int m_RoomCount = 3;

        List<LevelRoom> m_LevelRoomList = new List<LevelRoom>();

        public Level(LevelGraph sceneGraph)
        {
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

            for(int i =0;i < m_RoomCount;i++)
            {
                int meshCountInRoom = Random.Range(1, freeCount + 2);
                freeCount -= meshCountInRoom - 1;
                int allRandomCount = meshCount - 1 - alreadyRandomCount;
                int randomIndex = Random.Range(0, 100 * meshCount) % (allRandomCount);
                int randomResult = randomArray[randomIndex];
                randomArray[randomResult] = randomArray[allRandomCount];
                alreadyRandomCount++;

               
            }
        }

        void InitCorridor(LevelGraph sceneGraph)
        {

        }



    }
}

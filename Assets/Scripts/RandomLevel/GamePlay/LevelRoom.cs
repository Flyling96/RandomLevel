using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;


namespace DragonSlay.RandomLevel.Level
{
    public class LevelRoom : LevelArea
    {
        List<LevelMesh> m_RoomMesh = null;

        public bool m_IsEnd = false;

        public bool m_IsStart = false;

        public int m_Index = -1;

        public List<LevelRoom> m_NeighborList = new List<LevelRoom>();

        public LevelRoom(List<LevelMesh> roomMeshList)
        {
            m_RoomMesh = roomMeshList;

        }
    }
}

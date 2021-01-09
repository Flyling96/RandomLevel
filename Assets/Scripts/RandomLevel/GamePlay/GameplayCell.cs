using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Gameplay
{

    public class GameplayCell
    {
        public bool m_Walkable = false;

        public LevelArea m_Belong = null;

        public int m_GroupTypeMask = 0;

        public void MaskCell(LevelGroupType type)
        {
            m_GroupTypeMask |= 1 << (int)type;
        }

        public bool IsMaskCell(LevelGroupType type)
        {
            return (m_GroupTypeMask & 1 << (int)type) != 0;
        }
    }
}


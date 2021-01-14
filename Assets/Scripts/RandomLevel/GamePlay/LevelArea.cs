using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Gameplay
{

    public class LevelArea
    {
        public Vector3 m_Right;

        public Vector3 m_Up;

        public Vector3 m_Position;

        public HashSet<LevelCell> m_Cells = new HashSet<LevelCell>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public class LevelVoxel
    {
        public Vector3 m_Position;
        public int m_Size = 1;

        public virtual Mesh FillMesh()
        {
            return null;
        }
    }
}

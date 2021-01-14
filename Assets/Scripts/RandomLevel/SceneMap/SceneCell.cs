using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    public enum SceneCellType
    {
        None,
        Room,
        Corridor,
        Door,
        Wall,

        Max
    }
    public class SceneCell
    {
        public float m_Height = 0;
        public int m_CellTypeMask = 0;

        public void MaskCell(SceneCellType type)
        {
            m_CellTypeMask |= 1 << (int)type;
        }

        public void OnlyMaskCell(SceneCellType type)
        {
            m_CellTypeMask = 1 << (int)type;
        }

        public bool IsMaskCell(SceneCellType type)
        {
            return (m_CellTypeMask & 1 << (int)type) != 0;
        }

        public bool IsEqualMaskCell(SceneCellType[] types)
        {
            int mask = 0;
            for(int i = 0;i< types.Length;i++)
            {
                mask |= 1 << (int)types[i];
            }

            return m_CellTypeMask == mask;
        }
    }
}

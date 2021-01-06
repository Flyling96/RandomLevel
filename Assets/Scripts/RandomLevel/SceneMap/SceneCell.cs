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

        public bool IsMaskCell(SceneCellType type)
        {
            return (m_CellTypeMask & 1 << (int)type) != 0;
        }
    }
}

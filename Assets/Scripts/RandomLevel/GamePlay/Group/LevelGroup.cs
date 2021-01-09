using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonSlay.RandomLevel.Scene;

namespace DragonSlay.RandomLevel.Gameplay
{
    public enum LevelGroupType
    {
        None,
        Door,
        Monster,
        Trap,
    }

    public class LevelGroup : LevelArea
    {
        public virtual LevelGroupType GroupType => LevelGroupType.None;

        public virtual void FillGroup(LevelCell center) { }

        public void AddCell(LevelPanel shape, Dictionary<Vector3, LevelCell> cellDic, int cellSize)
        {
            AABoundingBox2D aabb2D = shape.GetAABB2D(cellSize);
            int minX = (int)aabb2D.m_Min.x;
            int minY = (int)aabb2D.m_Min.y;
            int maxX = (int)aabb2D.m_Max.x;
            int maxY = (int)aabb2D.m_Max.y;

            Vector3 right = shape.m_Right;
            Vector3 up = shape.m_Up;

            for (int y = minY; y < maxY + 1; y += cellSize)
            {
                for (int x = minX; x < maxX + 1; x += cellSize)
                {
                    LevelCell cell = null;
                    Vector2 cellCenter = new Vector2(x, y);
                    Vector3 cellPos = cellCenter.x * right + cellCenter.y * up;
                    if (cellDic.TryGetValue(cellPos, out cell))
                    {
                        if (cell.IsInMesh(shape))
                        {
                            m_Cells.Add(cell);
                        }
                    }
                }
            }
        }

        public void RemoveCell(LevelCell center,LevelPanel shape, Dictionary<Vector2, LevelCell> cellDic)
        {

        }
    }
}

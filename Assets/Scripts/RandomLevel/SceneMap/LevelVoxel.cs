using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public class LevelVoxel
    {
        public Vector3 m_Position;
        public int m_Size = 1;

        public HashSet<LevelMesh> m_ParentSet = new HashSet<LevelMesh>();

        public virtual Mesh ConvertMesh()
        {
            return null;
        }

        public virtual void FillMesh(List<Vector3> vertexList,List<int> triangleList, Vector3 startPos)
        {

        }

        public enum VertexColorType
        {
            Show,
        }

        public virtual void SetMeshColor(List<Color> colorList, VertexColorType colorType)
        {

        }

        Color GetVertexColor(VertexColorType colorType)
        {
            switch (colorType)
            {
                case VertexColorType.Show:
                    return Color.white;
            }
            return Color.black;
        }
    }
}

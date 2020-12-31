using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public enum LevelEdgeType
    {

    }

    public class LevelEdge : LevelMesh2D
    {

        Vector2 m_Start;
        Vector2 m_End;
        Vector2[] m_MidPoints;

        float m_EdgeWidth;

        public LevelEdge(UEdge2D data,float width)
        {
            m_Position = data.Points[0].Point.x * m_Right + data.Points[0].Point.y * m_Up;
            m_Position.y = 0.1f;
            m_Start = Vector2.zero;
            m_End = data.Points[1].Point - data.Points[0].Point;
            float RandomX = Random.Range(0, m_End.x);
            float RandomY = Random.Range(0, m_End.y);
            if(RandomY < RandomX)
            {
                m_MidPoints = new Vector2[2] { new Vector2(m_Start.x, RandomY), new Vector2(m_End.x, RandomY) };
            }
            else
            {
                m_MidPoints = new Vector2[2] { new Vector2(RandomX, m_Start.y), new Vector2(RandomX, m_End.y) };
            }
            //m_MidPoints = new Vector2[1] { new Vector2(m_Start.x,m_End.y)};
            m_EdgeWidth = width;
        }

        public LevelEdge(Vector2 start,Vector2 end,Vector2[] mids,float width)
        {
            m_Start = start;
            m_End = end;
            m_MidPoints = mids;
            m_EdgeWidth = width;
        }

        public void GenerateMesh()
        {
            Vector2 startPoint , midPoint, endPoint;
            startPoint = m_Start;
            endPoint = m_End;
            Vector2 edge0 ,edge1 = Vector2.zero;
            Vector2 startEdgeOffset, midEdgeOffset,endEdgeOffset;
            Vector2 startEdgePoint0 = Vector2.zero, startEdgePoint1 = Vector2.zero,
                midEdgePoint0, midEdgePoint1,endEdgePoint0,endEdgePoint1;

            List<Vector2> vertexList = new List<Vector2>();
            List<int> triangleList = new List<int>();
            vertexList.Add(startPoint);
            int startIndex = 0;

            for (int i =0;i<m_MidPoints.Length;i++)
            {
                midPoint = m_MidPoints[i];
                endPoint = i < m_MidPoints.Length - 1 ? m_MidPoints[i + 1] : m_End;
                edge0 = midPoint - startPoint;
                edge1 = endPoint - midPoint;
                if(startPoint == m_Start)
                {
                    startEdgeOffset = new Vector2(-edge0.y, edge0.x).normalized * m_EdgeWidth;
                    startEdgePoint0 = startPoint + startEdgeOffset;
                    startEdgePoint1 = startPoint - startEdgeOffset;
                    vertexList.Add(startEdgePoint0);
                    vertexList.Add(startEdgePoint1);
                }
                midEdgeOffset = CalculateMidPointOffset(edge0, edge1, m_EdgeWidth);
                midEdgePoint0 = midPoint + midEdgeOffset;
                midEdgePoint1 = midPoint - midEdgeOffset;
                vertexList.Add(midPoint);
                vertexList.Add(midEdgePoint0);
                vertexList.Add(midEdgePoint1);

                FillTriangle(startPoint, startEdgePoint0, midPoint, startIndex, startIndex + 1, startIndex + 3, triangleList);
                FillTriangle(startEdgePoint0, midEdgePoint0, midPoint, startIndex + 1, startIndex + 4, startIndex + 3, triangleList);
                FillTriangle(startPoint, midPoint, startEdgePoint1, startIndex, startIndex + 3, startIndex + 2, triangleList);
                FillTriangle(startEdgePoint1, midPoint, midEdgePoint1, startIndex + 2, startIndex + 3, startIndex + 5, triangleList);

                startPoint = midPoint;
                startEdgePoint0 = midEdgePoint0;
                startEdgePoint1 = midEdgePoint1;
                startIndex += 3;
            }
            
            if(startPoint == m_Start)
            {
                edge1 = endPoint - startPoint;
                startEdgeOffset = new Vector2(-edge1.y, edge1.x).normalized * m_EdgeWidth;
                startEdgePoint0 = startPoint + startEdgeOffset;
                startEdgePoint1 = startPoint - startEdgeOffset;
                vertexList.Add(startEdgePoint0);
                vertexList.Add(startEdgePoint1);
            }

            vertexList.Add(endPoint);
            endEdgeOffset = new Vector2(-edge1.y, edge1.x).normalized * m_EdgeWidth;
            endEdgePoint0 = endPoint + endEdgeOffset;
            endEdgePoint1 = endPoint - endEdgeOffset;
            vertexList.Add(endEdgePoint0);
            vertexList.Add(endEdgePoint1);

            FillTriangle(startPoint, startEdgePoint0, endPoint, startIndex, startIndex + 1, startIndex + 3, triangleList);
            FillTriangle(startEdgePoint0, endEdgePoint0, endPoint, startIndex + 1, startIndex + 4, startIndex + 3, triangleList);
            FillTriangle(startPoint, endPoint, startEdgePoint1, startIndex, startIndex + 3, startIndex + 2, triangleList);
            FillTriangle(startEdgePoint1, endPoint, endEdgePoint1, startIndex + 2, startIndex + 3, startIndex + 5, triangleList);

            Vector3[] vertexArray = new Vector3[vertexList.Count];
            for(int i =0;i< vertexList.Count;i++)
            {
                vertexArray[i] = vertexList[i].x * m_Right + vertexList[i].y * m_Up;
            }

            FillBorders(vertexList);

            m_Vertices = vertexArray;
            m_Triangles = triangleList.ToArray();
        }

        void FillBorders(List<Vector2> vertexList)
        {
            int centerPointCount = vertexList.Count / 3;
            List<Vector2> borderList = new List<Vector2>();
            for(int i = 0; i < centerPointCount; i++)
            {
                int index = i * 3 + 1;
                borderList.Add(vertexList[index]);
            }

            for(int i = centerPointCount - 1; i > -1; i --)
            {
                int index = i * 3 + 2;
                borderList.Add(vertexList[index]);
            }

            var v0 = borderList[1] - borderList[0];
            var v1 = borderList[2] - borderList[1];
            //逆时针反转
            if(v0.x * v1.y - v1.x * v0.y > 0)
            {
                List<Vector2> newBorderList = new List<Vector2>();
                for(int i = borderList.Count - 1; i > -1;i--)
                {
                    newBorderList.Add(borderList[i]);
                }
                m_Borders = newBorderList.ToArray();
            }
            else
            {
                m_Borders = borderList.ToArray();
            }
        }

        void FillTriangle(Vector2 p0,Vector2 p1,Vector2 p2,int t0,int t1,int t2,List<int> list)
        {
            var edge0 = p1 - p0;
            var edge1 = p2 - p1;
            float cross = edge0.x * edge1.y - edge1.x * edge0.y;
            list.Add(t0);
            if (cross < 0)
            {
                list.Add(t1);
                list.Add(t2);
            }
            else
            {
                list.Add(t2);
                list.Add(t1);
            }
        }

        public Vector2 CalculateMidPointOffset(Vector2 a, Vector2 b,float width)
        {
            a = a.normalized;
            b = b.normalized;
            Vector2 result;

            if(a == b)
            {
                result = new Vector2(-a.y, a.x) * width;
                return result;
            }

            if(a.x == -b.y && a.y == b.x)
            {
                result = a * width - b * width;
                return result;
            }

            float x = (b.x - a.x) / (a.x * b.y - b.x * a.y) * width;
            float y = (b.y - a.y) / (a.x * b.y - b.x * a.y) * width;
            result = new Vector2(x, y);
            if(result.magnitude > width * 3.0f)
            {
                result = result.normalized * width * 3.0f;
            }
            return result;
        }

    }
}

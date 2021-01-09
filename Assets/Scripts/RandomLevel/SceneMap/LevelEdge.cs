﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    public enum LevelEdgeType
    {
        Straight,
        Polyline,
        Bezier2,
    }

    public class LevelEdge : LevelMesh2D
    {

        Vector2 m_Start;
        Vector2 m_End;
        Vector2[] m_MidPoints;

        float m_EdgeWidth;

        public LevelEdge(Shape start, Shape end, List<Shape> shapeList, float width)
        {
            m_Position = start.m_Position.x * m_Right + start.m_Position.y * m_Up;
            m_Position += 0.1f * Vector3.Cross(m_Up, m_Right);
            m_Start = Vector2.zero;
            m_End = end.m_Position - start.m_Position;
            //m_MidPoints = new Vector2[0];
            m_EdgeWidth = width;
            //var mainDir = (end.m_Position - start.m_Position).normalized;
            //var startProj = Vector2.Dot(start.m_Position, mainDir);
            //var startPos = start.m_Position + (SeparatingAxisAlgorithm.GetProjection(start, mainDir, false).y - startProj) * mainDir;
            //var endProj = Vector2.Dot(end.m_Position, mainDir);
            //var endPos = end.m_Position + (SeparatingAxisAlgorithm.GetProjection(end, mainDir, false).x - endProj) * mainDir;
            //m_Position = startPos.x * m_Right + startPos.y * m_Up;
            //m_Position += 0.1f * Vector3.Cross(m_Up, m_Right);
            //m_Start = Vector2.zero;
            //m_End = endPos - startPos;
            FillBezier2MidPoint(start,end, shapeList, 100);

            //Vector2[] mainMidPoints = null;
            //float RandomX = Random.Range(0, m_End.x);
            //float RandomY = Random.Range(0, m_End.y);
            //if (RandomY < RandomX)
            //{
            //    mainMidPoints = new Vector2[2] { new Vector2(m_Start.x, RandomY), new Vector2(m_End.x, RandomY) };
            //}
            //else
            //{
            //    mainMidPoints = new Vector2[2] { new Vector2(RandomX, m_Start.y), new Vector2(RandomX, m_End.y) };
            //}

            //FillPolyLineMidPoint(start, end, shapeList, 100, mainMidPoints);
        }

        public LevelEdge(UEdge2D data,float width)
        {
            //m_Data = data;
            m_Position = data.Point0.Point.x * m_Right + data.Point0.Point.y * m_Up;
            m_Position += 0.1f * Vector3.Cross(m_Up, m_Right);
            m_Start = Vector2.zero;
            m_End = data.Point1.Point - data.Point0.Point;
            m_MidPoints = new Vector2[0];
            //FillBezier2MidPoint(10);
            //float RandomX = Random.Range(0, m_End.x);
            //float RandomY = Random.Range(0, m_End.y);
            //if (RandomY < RandomX)
            //{
            //    m_MidPoints = new Vector2[2] { new Vector2(m_Start.x, RandomY), new Vector2(m_End.x, RandomY) };
            //}
            //else
            //{
            //    m_MidPoints = new Vector2[2] { new Vector2(RandomX, m_Start.y), new Vector2(RandomX, m_End.y) };
            //}
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

        public override void GenerateVoxel(int voxelSize)
        {
            int pointCount = m_Borders.Length / 2;
            HashSet<Vector2> voxelCenterSet = new HashSet<Vector2>();
            AABoundingBox2D[] aabb2Ds = GetAABB2Ds(voxelSize);
            for (int i = 0; i < pointCount - 1; i++)
            {
                var aabb2D = aabb2Ds[i];

                int minX = (int)aabb2D.m_Min.x ;
                int minY = (int)aabb2D.m_Min.y;
                int maxX = (int)aabb2D.m_Max.x;
                int maxY = (int)aabb2D.m_Max.y;

                for (int j = minY; j < maxY + 1; j += voxelSize)
                {
                    for (int k = minX; k < maxX + 1; k += voxelSize)
                    {
                        voxelCenterSet.Add(new Vector2(k, j));
                    }
                }
            }

            foreach(var center in voxelCenterSet)
            {
                LevelCell levelCell = new LevelCell(center, m_Right, m_Up, voxelSize);
                if (Mathf.Abs(center.x) < voxelSize && Mathf.Abs(center.y) < voxelSize)
                {
                    m_StartVoxel = levelCell;
                }
                m_Voxels.Add(levelCell);
            }
        }

        public virtual AABoundingBox2D[] GetAABB2Ds(int voxelSize)
        {
            int pointCount = m_Borders.Length / 2;
            AABoundingBox2D[] aabb2Ds = new AABoundingBox2D[pointCount - 1];
            for (int i = 0; i < pointCount - 1; i++)
            {
                Vector2 v0 = m_Borders[i] ;
                Vector2 v1 = m_Borders[i + 1];
                Vector2 v2 = m_Borders[m_Borders.Length - 1 - i];
                Vector2 v3 = m_Borders[m_Borders.Length - 2 - i];

                var aabb2D = new AABoundingBox2D(new Vector2[4] { v0, v1, v2, v3 });

                aabb2D.m_Min.x = Mathf.FloorToInt(aabb2D.m_Min.x / voxelSize) * voxelSize;
                aabb2D.m_Min.y = Mathf.FloorToInt(aabb2D.m_Min.y / voxelSize) * voxelSize;
                aabb2D.m_Max.x = Mathf.CeilToInt(aabb2D.m_Max.x / voxelSize) * voxelSize;
                aabb2D.m_Max.y = Mathf.CeilToInt(aabb2D.m_Max.y / voxelSize) * voxelSize;

                aabb2Ds[i] = aabb2D;
            }
            return aabb2Ds;
        }

        public override bool IsPointInside(Vector2 point)
        {
            Vector2 p0, p1, v0, v1;
            float cross = 1;
            //线段非凸包，需要进行拆解
            int pointCount = m_Borders.Length / 2;
            Vector2 mainDir = m_End - m_Start;

            for (int j = 0; j < pointCount - 1; j++)
            {
                Vector2[] vertices = new Vector2[4];
                vertices[0] = m_Borders[j];
                vertices[1] = m_Borders[j + 1];
                vertices[2] = m_Borders[m_Borders.Length - 2 - j];
                vertices[3] = m_Borders[m_Borders.Length - 1 - j];

                //大拐弯的内弯处可能会出现方向颠倒的问题，需要矫正
                var v01 = vertices[1] - vertices[0];
                var v23 = vertices[2] - vertices[3];
                cross = v01.x * mainDir.y - mainDir.x * v01.y;
                if (cross < 0)
                {
                    var temp = vertices[0];
                    vertices[0] = vertices[1];
                    vertices[1] = temp;
                }

                cross = v23.x * mainDir.y - mainDir.x * v23.y;
                if (cross < 0)
                {
                    var temp = vertices[2];
                    vertices[2] = vertices[3];
                    vertices[3] = temp;
                }

                cross = 1;
                bool isOut = false;

                for (int i = 0; i < vertices.Length; i++)
                {
                    p0 = vertices[i];
                    p1 = vertices[(i + 1) % vertices.Length];
                    v0 = p1 - p0;
                    v1 = point - p0;
                    float temp = v0.x * v1.y - v1.x * v0.y;
                    if (i != 0)
                    {
                        if (temp * cross <= 0)
                        {
                            isOut = true;
                            break;
                        }
                    }
                    cross = temp;
                }

                if(!isOut)
                {
                    return true;
                }
            }
            return false;
        }
        //public override Vector3 CalculateVoxelMeshPos(Vector3 pos, int voxelSize)
        //{
        //    return pos;
        //}

        #region Curve

        int MidPointCollsion(Shape start, Shape end, List<Shape> shapeList, Vector2[] midpoints)
        {
            HashSet<Shape> collsionShape = new HashSet<Shape>();
            for (int j = 0; j < midpoints.Length; j++)
            {
                var midPoint = midpoints[j];
                for (int i = 0; i < shapeList.Count; i++)
                {
                    var shape = shapeList[i];
                    var positionOffset = shape.m_Position - start.m_Position;
                    if (shape == start || shape == end)
                    {
                        continue;
                    }

                    bool oldCanMove = shapeList[i].m_CanMove;
                    float oldSkinWidth = shapeList[i].m_SkinWidth;
                    shapeList[i].m_CanMove = false;
                    shapeList[i].m_SkinWidth = 0;
                    Circle circle = new Circle(midPoint + start.m_Position, m_EdgeWidth,0);
                    if (SeparatingAxisAlgorithm.SeparatingAxis(shapeList[i], circle))
                    {
                        midpoints[j] = circle.m_Position - start.m_Position;
                        collsionShape.Add(shapeList[i]);
                    }
                    shapeList[i].m_CanMove = oldCanMove;
                    shapeList[i].m_SkinWidth = oldSkinWidth;
                }
            }

            return collsionShape.Count;
        }

        int MidPointSphereCollsion(Shape start, Shape end, List<Shape> shapeList, Vector2[] midpoints)
        {
            HashSet<Shape> collsionShape = new HashSet<Shape>();
            for (int j = 0; j < midpoints.Length; j++)
            {
                var midPoint = midpoints[j];
                for (int i = 0; i < shapeList.Count; i++)
                {
                    var shape = shapeList[i];
                    var positionOffset = shape.m_Position - start.m_Position;
                    if (shape == start || shape == end)
                    {
                        continue;
                    }
                    float sphereDistance = shape.GetBoundingSphereRadius() + m_EdgeWidth + 1;
                    if (Vector2.Distance(midPoint, positionOffset) < sphereDistance)
                    {
                        midpoints[j] = positionOffset + (midPoint - positionOffset).normalized * sphereDistance;
                        collsionShape.Add(shape);
                        break;
                    }
                }
            }

            return collsionShape.Count;
        }

        void FillPolyLineMidPoint(Shape start, Shape end, List<Shape> shapeList, int midPointCount,Vector2[] midMainPoints)
        {
            Vector2[] mainPoints = new Vector2[midMainPoints.Length + 2];
            mainPoints[0] = m_Start; mainPoints[mainPoints.Length - 1] = m_End;
            for(int i = 1; i < mainPoints.Length -1;i++)
            {
                mainPoints[i] = midMainPoints[i - 1];
            }

            var midPoints = FillPolyLineMidPoint(midPointCount, mainPoints);
            int collsionCount = MidPointCollsion(start, end, shapeList, midPoints);
            m_MidPoints = midPoints;
        }

        Vector2[] FillPolyLineMidPoint(int midPointCount, Vector2[] mainPoints)
        {
            Vector2[] midPoints = new Vector2[midPointCount];

            float[] distances = new float[mainPoints.Length - 1];
            float allDistance = 0;
            for(int i = 1;i< mainPoints.Length;i++)
            {
                distances[i - 1] = Vector2.Distance(mainPoints[i - 1], mainPoints[i]);
                allDistance += distances[i - 1];
            }

            int[] pointCounts = new int[mainPoints.Length - 1];
            int allPointCount = 0;
            for(int i =0;i< pointCounts.Length -1;i++)
            {
                pointCounts[i] = (int)(distances[i] / allDistance * (midPointCount + 1));
                allPointCount += pointCounts[i];
            }
            pointCounts[pointCounts.Length - 1] = midPointCount + 1 - allPointCount;

            int index = 0;
            for(int i =0;i< pointCounts.Length;i++)
            {
                var start = mainPoints[i];
                var end = mainPoints[i + 1];
                int pointCount = pointCounts[i];
                for(int j = 1; j < pointCount + 1;j++)
                {
                    if (index < midPointCount)
                    {
                        midPoints[index] = Vector2.Lerp(start, end, (float)j / pointCount);
                        index++;
                    }
                }
            }

            return midPoints;

        }

        void FillBezier2MidPoint(Shape start, Shape end, List<Shape> shapeList,int midPointCount)
        {
            var midPointRight = FillBezier2MidPoint(midPointCount, true);
            var midPointLeft = FillBezier2MidPoint(midPointCount, false);
            int rightCount = MidPointSphereCollsion(start, end, shapeList, midPointRight);
            int leftCount = MidPointSphereCollsion(start, end, shapeList, midPointLeft);
            if(leftCount < rightCount)
            {
                m_MidPoints = midPointLeft;
            }
            else
            {
                m_MidPoints = midPointRight;
            }

        }

        Vector2[] FillBezier2MidPoint(int midPointCount,bool right)
        {
            var midPoints = new Vector2[midPointCount];
            Vector2 p1 = right ? new Vector2(m_Start.x, m_End.y) : new Vector2(m_End.x, m_Start.y);
            for(int i =0;i< midPointCount;i++)
            {
                Vector2 midPoint = Bezier2(m_Start, p1, m_End, ((float)i + 1) / (midPointCount + 2));
                midPoints[i] = midPoint;
            }
            return midPoints;
        }

        Vector2 Bezier2(Vector2 p0,Vector2 p1, Vector2 p2,float t)
        {
            Vector2 p0p1 = (1 - t) * p0 + t * p1;
            Vector2 p1p2 = (1 - t) * p1 + t * p2;
            Vector2 res = (1 - t) * p0p1 + t * p1p2;
            return res;
        }
        #endregion

    }
}

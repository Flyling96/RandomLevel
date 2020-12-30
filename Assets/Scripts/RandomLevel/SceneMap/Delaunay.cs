
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public class UVertex2D
    {

        /// <summary>
        /// id < 0 表示超级三角形的顶点，否则表示正常的顶点
        /// </summary>
        public int Id;

        /// <summary>
        /// 按照水平轴进行排序，SortBy = Point.x 
        /// </summary>
        public float SortBy;

        /// <summary>
        /// 顶点
        /// </summary>
        public Vector2 Point;

        public UVertex2D(int _id, Vector2 _point)
        {
            Id = _id;
            Point = _point;
            SortBy = _point.x;
        }

    }

    public class UEdge2D
    {
        /// <summary>
        /// 一条边的两个顶点
        /// </summary>
        public UVertex2D[] Points { get; private set; }

        /// <summary>
        /// 线段的长度
        /// </summary>
        public float Distance { get; private set; }

        /// <summary>
        /// 这条边的全局唯一名称
        /// </summary>
        public string FromName { get; private set; }
        public string ToName { get; private set; }

        public UEdge2D(UVertex2D _from, UVertex2D _to)
        {
            Points = new UVertex2D[2];
            Points[0] = _from;
            Points[1] = _to;
            Distance = Vector2.Distance(Points[0].Point, Points[1].Point);
            FromName = _from.Id + "_" + _to.Id;
            ToName = _to.Id + "_" + _from.Id;
        }

        /// <summary>
        /// 检测两条边使用的顶点是否相同，如果是那么这两条边是共享边
        /// </summary>
        /// <param name="_other"></param>
        /// <returns></returns>
        public bool IsEquals(UEdge2D _other)
        {
            return _other.ContainId(Points[0].Id) && _other.ContainId(Points[1].Id);
        }

        bool ContainId(int _vertexid)
        {
            return Points[0].Id == _vertexid || Points[1].Id == _vertexid;
        }
    }

    public class UTriangle2D
    {
        /// <summary>
        /// 三角形顶点集合
        /// </summary>    
        public UVertex2D[] Points = new UVertex2D[3];
        /// <summary>
        /// 三角形边集合
        /// </summary>    
        public UEdge2D[] Edges = new UEdge2D[3];
        /// <summary>
        /// 最长的边
        /// </summary>
        public UEdge2D Max { get; private set; }
        /// <summary>
        /// 最短的边
        /// </summary>
        public UEdge2D Min { get; private set; }
        /// <summary>
        /// 三角形外接圆的圆心
        /// </summary>
        public Vector2 CircleCenter { get; private set; }
        /// <summary>
        /// 三角形外接圆的半径
        /// </summary>
        public float Radius { get; private set; }
        /// <summary>
        /// 是否为超级三角形的一部分，如果包含带有负数的顶点Id
        /// 则认为这是一个超级三角形的组成成员
        /// </summary>
        public bool IsSuper { get; private set; }

        public UTriangle2D(UVertex2D _a, UVertex2D _b, UVertex2D _c)
        {
            Points[0] = _a;
            Points[1] = _b;
            Points[2] = _c;
            var e0 = new UEdge2D(_a, _b);
            var e1 = new UEdge2D(_a, _c);
            var e2 = new UEdge2D(_b, _c);
            Max = e0;
            Min = e0;
            if (e1.Distance > Max.Distance) Max = e1;
            if (e2.Distance > Max.Distance) Max = e2;
            if (e1.Distance < Min.Distance) Min = e1;
            if (e2.Distance < Min.Distance) Min = e2;
            Edges[0] = e0;
            Edges[1] = e1;
            Edges[2] = e2;
            CircleCenter = GetCircleCenter();
            Radius = (Points[0].Point - CircleCenter).magnitude;
            if (Points[0].Id < 0 || Points[1].Id < 0 || Points[2].Id < 0)
            {
                IsSuper = true;
            }
        }

        /// <summary>
        /// 外接圆是否在顶点的左边
        /// </summary>
        /// <param name="_vertexX"></param>
        /// <returns></returns>
        public bool IsLeftOf(float _vertexX)
        {
            return _vertexX > (CircleCenter.x + Radius);
        }

        /// <summary>
        /// 判断某个顶点是否在外接圆之内
        /// </summary>
        /// <param name="_vertex"></param>
        /// <returns></returns>
        public bool InCircumscribedCircle(UVertex2D _vertex)
        {
            float r2 = (_vertex.Point - CircleCenter).magnitude;
            return r2 < Radius;
        }

        /// <summary>
        /// 获取外接圆的中心点
        /// </summary>
        /// <returns></returns>
        Vector2 GetCircleCenter()
        {
            Vector2 p1 = Points[0].Point, p2 = Points[1].Point, p3 = Points[2].Point;

            float D = (p1.x * (p2.y - p3.y) +
                        p2.x * (p3.y - p1.y) +
                        p3.x * (p1.y - p2.y)) * 2;
            float x = ((p1.x * p1.x + p1.y * p1.y) * (p2.y - p3.y) +
                        (p2.x * p2.x + p2.y * p2.y) * (p3.y - p1.y) +
                        (p3.x * p3.x + p3.y * p3.y) * (p1.y - p2.y));
            float y = ((p1.x * p1.x + p1.y * p1.y) * (p3.x - p2.x) +
                        (p2.x * p2.x + p2.y * p2.y) * (p1.x - p3.x) +
                        (p3.x * p3.x + p3.y * p3.y) * (p2.x - p1.x));
            return new Vector2((x / D), (y / D));
        }
    }

    public class UDelaunayBestResult
    {
        public List<UVertex2D> Vertexes;
        public List<UTriangle2D> Triangles;
        public List<UEdge2D> Edges;
    }

    public class UDelaunayBest
    {
        public static UDelaunayBestResult GetTriangles2D(List<UVertex2D> _vertexes)
        {
            // 存储已经完成的三角形集合
            List<UTriangle2D> complete = new List<UTriangle2D>();
            // 还需要参与运算的三角形集合
            List<UTriangle2D> doing = new List<UTriangle2D>();

            // 按照水平轴进行排序
            _vertexes.Sort((a, b) =>
            {
                return a.SortBy.CompareTo(b.SortBy);
            });

            int newId = 0;
            foreach (var v in _vertexes)
            {
                v.Id = newId++;
            }

            // 找到最小和最大的点
            float minX = 0, minY = 0, maxX = 0, maxY = 0;
            foreach (var p in _vertexes)
            {
                Vector2 v = p.Point;
                if (v.x < minX) minX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.x > maxX) maxX = v.x;
                if (v.y > maxY) maxY = v.y;
            }

            minX -= 10;
            minY -= 10;
            maxX += 10;
            maxY += 10;

            // 创建超级三角形
            UVertex2D leftUp = new UVertex2D(-1, new Vector2(minX, maxY));
            UVertex2D rightUp = new UVertex2D(-2, new Vector2(maxX, maxY));
            UVertex2D rightDown = new UVertex2D(-3, new Vector2(maxX, minY));
            UVertex2D leftDown = new UVertex2D(-4, new Vector2(minX, minY));

            // 为了确保所有的点都包含在三角形内
            // 这里使用了两个超级三角形拼成的矩形
            doing.Add(new UTriangle2D(leftUp, rightUp, rightDown));
            doing.Add(new UTriangle2D(leftUp, rightDown, leftDown));

            // 逐个添加顶点
            foreach (var v in _vertexes)
            {
                AddVertex(doing, complete, v);
            }

            // 将剩余不是超级三角形的三角形添加到三角形队列
            foreach (var v in doing)
            {
                if (v.IsSuper == false)
                    complete.Add(v);
            }

            UDelaunayBestResult result = new UDelaunayBestResult();
            result.Vertexes = _vertexes;
            result.Triangles = complete;
            result.Edges = GetEdgetsFromTriangles(complete);
            return result;
        }

        static void AddVertex(
            List<UTriangle2D> _doing,
            List<UTriangle2D> _complete,
            UVertex2D _vertex)
        {
            List<UEdge2D> edges = new List<UEdge2D>();
            UTriangle2D temp;

            for (int i = 0; i < _doing.Count; i++)
            {
                temp = _doing[i];
                // 检查三角形外接圆是否包含 _vertex
                // 如果是，就从工作队列中移除它，并记录它所有的边
                if (temp.InCircumscribedCircle(_vertex))
                {
                    _doing.RemoveAt(i--);
                    edges.AddRange(temp.Edges);
                }
                /* 如果当前三角形不是超级三角形的组成部分
                并且外接圆位完全位于新顶点的左侧，那么
                根据顶点x轴的排序规则，将永远不会有新的顶点
                进入到temp的外接圆内，此时，它被认定为一个
                已经完成的三角形*/
                else if (false == temp.IsSuper && temp.IsLeftOf(_vertex.Point.x))
                {
                    _complete.Add(temp);
                    _doing.RemoveAt(i--);
                }
            }

            // 移除双边
            for (int i = 0; i < edges.Count; i++)
            {
                var ei = edges[i];
                for (int n = i + 1; n < edges.Count; n++)
                {
                    var en = edges[n];
                    if (ei.IsEquals(en))
                    {
                        edges.RemoveAt(n);
                        edges.RemoveAt(i--);
                        break;
                    }
                }
            }

            // 创建新的三角形
            foreach (var v in edges)
            {
                _doing.Add(new UTriangle2D(v.Points[0], v.Points[1], _vertex));
            }
        }

        /// <summary>
        /// 根据三角面片创建唯一的边
        /// </summary>
        /// <param name="_list"></param>
        /// <returns></returns>
        static List<UEdge2D> GetEdgetsFromTriangles(List<UTriangle2D> _list)
        {
            Dictionary<string, UEdge2D> maps = new Dictionary<string, UEdge2D>();
            foreach (var v in _list)
            {
                var a = v.Edges[0];
                var b = v.Edges[1];
                var c = v.Edges[2];
                maps[a.FromName] = a;
                maps[a.ToName] = null;
                maps[b.FromName] = b;
                maps[b.ToName] = null;
                maps[c.FromName] = c;
                maps[c.ToName] = null;
            }

            List<UEdge2D> edges = new List<UEdge2D>();
            edges.AddRange(maps.Values);
            edges.RemoveAll((v) => v == null);
            return edges;
        }
    }
}
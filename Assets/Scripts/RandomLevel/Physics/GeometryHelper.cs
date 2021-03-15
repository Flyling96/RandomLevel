using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape
{
    public Vector2 m_Position;

    public Vector3 m_Normal;

    public float m_SkinWidth;

    public int m_Pole;

    public bool m_CanMove = true;

    public virtual float GetBoundingSphereRadius(bool isSkinWidth = false)
    {
        return 0;
    }
}

public class Circle: Shape
{

    public float m_Radius;

    public Circle(Vector2 pos,float radius,float skinWidth)
    {
        m_Position = pos;
        m_Radius = radius;
        m_SkinWidth = skinWidth;
        m_Pole = UnityEngine.Random.Range(0, 500) % 3;
    }

    public override float GetBoundingSphereRadius(bool isSkinWidth = false)
    {
        return isSkinWidth ? m_Radius + m_SkinWidth : m_Radius;
    }

    public void Move(Vector2 movePos)
    {
        m_Position += movePos;
    }
}

public class Polygon : Shape
{

    public Vector2 m_Center;

    Vector2[] m_Borders;

    Vector2[] m_SkinBorders;


    public Polygon(Vector2 center,Vector2[] borders,Vector2 position,Vector3 normal,float skinWidth)
    {
        m_SkinWidth = skinWidth;
        m_Center = center;
        m_Borders = borders;
        m_SkinBorders = new Vector2[m_Borders.Length];
        for (int i =0;i< m_SkinBorders.Length;i++)
        {
            m_SkinBorders[i] += m_Borders[i] + (m_Borders[i] - m_Center).normalized * m_SkinWidth;
        }
        m_Position = position;
        m_Normal = normal;
        m_Pole = UnityEngine.Random.Range(0, 500) % 3;
    }

    public void Move(Vector2 movePos)
    {
        m_Position += movePos;
    }

    public Vector2[] GetAxes()
    {
        Vector2[] axes = new Vector2[m_Borders.Length];
        for(int i =0;i< axes.Length;i++)
        {
            var p0 = m_Borders[i];
            var p1 = m_Borders[(i+1)% axes.Length];
            var edge = p1 - p0;
            edge = edge.normalized;
            axes[i] = new Vector2(-edge.y,edge.x);
        }
        return axes;
    }

    public Vector2[] GetBordersPoints(bool isSkinWidth = false)
    {
        Vector2[] points = new Vector2[m_Borders.Length];
        var borders = isSkinWidth ? m_SkinBorders : m_Borders;
        for (int i =0;i<points.Length;i++)
        {
            points[i] = m_Position + borders[i];
        }
        return points;
    }

    public override float GetBoundingSphereRadius(bool isSkinWidth = false)
    {
        float radius = 0;
        var borders = isSkinWidth ? m_SkinBorders : m_Borders;
        for(int i =0;i< borders.Length;i++)
        {
            var length = (m_Center - borders[i]).magnitude;
            if(length > radius)
            {
                radius = length;
            }
        }
        return radius;
    }
}

public struct Triangle
{
    public Vector2 p0, p1, p2;

    public Triangle(Vector2 t0,Vector2 t1,Vector2 t2)
    {
        p0 = t0;
        p1 = t1;
        p2 = t2;
    }
}


public class GeometryHelper 
{
    #region SeparatingAxis
    public static float AttractStrength = 2.5f;

    public static bool IsShapesIntersect(Shape s0,Shape s1, bool isSkinWidth = false)
    {
        if (s0 is Polygon p0)
        {
            if (s1 is Polygon p1)
            {
                return IsShapesIntersect(p0, p1, isSkinWidth);
            }
            else if (s1 is Circle c0)
            {
                return IsShapesIntersect(p0, c0, isSkinWidth);
            }
        }
        else if (s0 is Circle c0)
        {
            if (s1 is Polygon p1)
            {
                return IsShapesIntersect(p1, c0, isSkinWidth);
            }
            else if (s1 is Circle c1)
            {
                return IsShapesIntersect(c0, c1, isSkinWidth);
            }
        }

        return false;
    }

    public static bool IsShapesIntersect(Circle c0, Circle c1,bool isSkinWidth = false)
    {
        float distance = (c0.m_Position - c1.m_Position).magnitude;
        float target = c0.GetBoundingSphereRadius(isSkinWidth) + c1.GetBoundingSphereRadius(isSkinWidth);
        if (distance > target)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool IsShapesIntersect(Polygon p0, Circle c0, bool isSkinWidth = false)
    {
        float distance = (p0.m_Position - c0.m_Position).magnitude;
        if (distance > p0.GetBoundingSphereRadius(isSkinWidth) + c0.GetBoundingSphereRadius(isSkinWidth))
        {
            return false;
        }

        Vector2[] axes0 = p0.GetAxes();
        Vector2 p0Proj, c0Proj;
        for (int i = 0; i < axes0.Length; i++)
        {
            p0Proj = GetProjection(p0, axes0[i], isSkinWidth);
            c0Proj = GetProjection(c0, axes0[i], isSkinWidth);

            if (!DoProjectionsOverlap(p0Proj, c0Proj))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsShapesIntersect(Polygon p0, Polygon p1, bool isSkinWidth = false)
    {
        float distance = (p0.m_Position - p1.m_Position).magnitude;
        if (distance > p0.GetBoundingSphereRadius(isSkinWidth) + p1.GetBoundingSphereRadius(isSkinWidth))
        {
            return false;
        }

        Vector2[] axes0 = p0.GetAxes();
        Vector2[] axes1 = p1.GetAxes();

        Vector2 p0Proj;
        Vector2 p1Proj;


        for (int i = 0; i < axes0.Length; i++)
        {
            p0Proj = GetProjection(p0, axes0[i], isSkinWidth);
            p1Proj = GetProjection(p1, axes0[i], isSkinWidth);
            if (!DoProjectionsOverlap(p0Proj, p1Proj))
            {
                return false;
            }
        }

        for (int i = 0; i < axes1.Length; i++)
        {
            p0Proj = GetProjection(p0, axes1[i], isSkinWidth);
            p1Proj = GetProjection(p1, axes1[i], isSkinWidth);
            if (!DoProjectionsOverlap(p0Proj, p1Proj))
            {
                return false;
            }
        }
        return true;
    }

    public static bool SeparatingAxis(Shape s0, Shape s1)
    {
        if (s0 is Polygon p0)
        {
            if (s1 is Polygon p1)
            {
                return SeparatingAxis(p0, p1);
            }
            else if(s1 is Circle c0)
            {
                return SeparatingAxis(p0, c0);
            }
        }
        else if(s0 is Circle c0)
        {
            if(s1 is Polygon p1)
            {
                return SeparatingAxis(p1, c0);
            }
            else if(s1 is Circle c1)
            {
                return SeparatingAxis(c0, c1);
            }
        }

        return false;
    }

    public static bool SeparatingAxis(Circle c0, Circle c1)
    {
        bool isRepulsive = c0.m_Pole != 1 || c1.m_Pole != 1;
        float distance = (c0.m_Position - c1.m_Position).magnitude;
        float target = c0.GetBoundingSphereRadius(isRepulsive) + c1.GetBoundingSphereRadius(isRepulsive);
        if (distance > target)
        {
            return false;
        }

        if (!c0.m_CanMove && !c1.m_CanMove)
        {
            return false;
        }

        Vector2 mainDir = c0.m_Position - c1.m_Position;
        if (mainDir == Vector2.zero)
        {
            mainDir = new Vector2(1, 0);
        }
        Vector2 moveDir = mainDir.normalized * (target - distance)/2;

        if (!isRepulsive)
        {
            moveDir -= moveDir.normalized * AttractStrength;
        }

        if (c1.m_CanMove && c0.m_CanMove)
        {
            c1.Move(-moveDir);
            c0.Move(moveDir);
        }
        else if (c1.m_CanMove)
        {
            c1.Move(-2 * moveDir);
        }
        else
        {
            c0.Move(2 * moveDir);
        }

        return true;
    }

    public static bool SeparatingAxis(Polygon p0, Circle c0)
    {
        bool isRepulsive = p0.m_Pole != 1 || c0.m_Pole != 1;

        float distance = (p0.m_Position - c0.m_Position).magnitude;
        if (distance > p0.GetBoundingSphereRadius(isRepulsive) + c0.GetBoundingSphereRadius(isRepulsive))
        {
            return false;
        }

        if (!p0.m_CanMove && !c0.m_CanMove)
        {
            return false;
        }

        Vector2[] axes0 = p0.GetAxes();
        Vector2 p0Proj, c0Proj;
        float projValue = float.MaxValue;
        Vector2 projAxis = Vector2.zero;

        for (int i = 0; i < axes0.Length; i++)
        {
            p0Proj = GetProjection(p0, axes0[i], isRepulsive);
            c0Proj = GetProjection(c0, axes0[i], isRepulsive);

            if (!DoProjectionsOverlap(p0Proj, c0Proj))
            {
                return false;
            }
            else
            {
                float value = CalculateOverlap(p0Proj, c0Proj);
                if (value < projValue)
                {
                    projValue = value;
                    projAxis = axes0[i];
                }
            }
        }

        Vector2 mainDir = c0.m_Position - p0.m_Position;
        if(mainDir == Vector2.zero)
        {
            mainDir = new Vector2(1, 0);
        }

        Vector2 moveDir =CaculateMoveDir(projAxis,projValue,mainDir)/2;

        if (!isRepulsive)
        {
            moveDir -= moveDir.normalized * AttractStrength;
        }

        if (p0.m_CanMove && c0.m_CanMove)
        {
            p0.Move(-moveDir);
            c0.Move(moveDir);
        }
        else if (p0.m_CanMove)
        {
            p0.Move(-2 * moveDir);
        }
        else
        {
            c0.Move(2 * moveDir);
        }

        return true;
    }

    public static bool SeparatingAxis(Polygon p0,Polygon p1)
    {
        bool isRepulsive = p0.m_Pole != 1 ||  p1.m_Pole != 1;

        float distance = (p0.m_Position - p1.m_Position).magnitude;
        if(distance > p0.GetBoundingSphereRadius(isRepulsive) + p1.GetBoundingSphereRadius(isRepulsive))
        {
            return false;
        }

        if(!p0.m_CanMove && !p1.m_CanMove)
        {
            return false;
        }

        Vector2[] axes0 = p0.GetAxes();
        Vector2[] axes1 = p1.GetAxes();

        Vector2 p0Proj;
        Vector2 p1Proj;

        float projValue = float.MaxValue;
        Vector2 projAxis = Vector2.zero;

        for(int i =0;i< axes0.Length; i++)
        {
            p0Proj = GetProjection(p0, axes0[i], isRepulsive);
            p1Proj = GetProjection(p1, axes0[i], isRepulsive);
            if(!DoProjectionsOverlap(p0Proj, p1Proj))
            {
                return false;
            }
            else
            {
                float value = CalculateOverlap(p0Proj, p1Proj);
                if(value < projValue)
                {
                    projValue = value;
                    projAxis = axes0[i];
                }
            }
        }

        for (int i = 0; i < axes1.Length; i++)
        {
            p0Proj = GetProjection(p0, axes1[i], isRepulsive);
            p1Proj = GetProjection(p1, axes1[i], isRepulsive);
            if (!DoProjectionsOverlap(p0Proj, p1Proj))
            {
                return false;
            }
            else
            {
                float value = CalculateOverlap(p0Proj, p1Proj);
                if (value < projValue)
                {
                    projValue = value;
                    projAxis = axes1[i];
                }
            }
        }

        Vector2 mainDir = p1.m_Position - p0.m_Position;
        if (mainDir == Vector2.zero)
        {
            mainDir = new Vector2(1, 0);
        }

        Vector2 moveDir = CaculateMoveDir(projAxis, projValue, mainDir) / 2;

        if (!isRepulsive)
        {
            moveDir -= moveDir.normalized * AttractStrength;
        }

        if (p0.m_CanMove && p1.m_CanMove)
        {
            p0.Move(-moveDir);
            p1.Move(moveDir);
        }
        else if(p0.m_CanMove)
        {
            p0.Move(-2 * moveDir);
        }
        else
        {
            p1.Move(2 * moveDir);
        }

        return true;
    }

    static Vector2 CaculateMoveDir(Vector2 axis,float moveValue,Vector2 mainDir)
    {
        float angle = Mathf.Atan(axis.y / axis.x);
        Vector2 res = new Vector2(Mathf.Cos(angle) * moveValue, Mathf.Sin(angle) * moveValue);
        if(res.x * mainDir.x + res.y * mainDir.y < 0)
        {
            res.x = -res.x;
            res.y = -res.y;
        }
        return res;
    }

    public static float SeparatingAxisDistance(Polygon p0, Polygon p1)
    {

        Vector2[] axes0 = p0.GetAxes();
        Vector2[] axes1 = p1.GetAxes();

        Vector2 p0Proj;
        Vector2 p1Proj;

        float projValue = float.MaxValue;
        Vector2 projAxis = Vector2.zero;

        for (int i = 0; i < axes0.Length; i++)
        {
            p0Proj = GetProjection(p0, axes0[i],false);
            p1Proj = GetProjection(p1, axes0[i],false);
            float value = CalculateMinDis(p0Proj, p1Proj);
            if (value < projValue)
            {
                projValue = value;
                projAxis = axes0[i];
            }
        }

        for (int i = 0; i < axes1.Length; i++)
        {
            p0Proj = GetProjection(p0, axes1[i],false);
            p1Proj = GetProjection(p1, axes1[i],false);
            float value = CalculateMinDis(p0Proj, p1Proj);
            if (value < projValue)
            {
                projValue = value;
                projAxis = axes1[i];
            }
        }

        return (projValue * projAxis).magnitude;
    }

    public static Vector2 GetProjection(Shape shape, Vector2 axis, bool isSkinWidth)
    {
        if(shape is Polygon polygon)
        {
            return GetProjection(polygon, axis, isSkinWidth);
        }
        else if(shape is Circle circle)
        {
            return GetProjection(circle, axis, isSkinWidth);
        }
        return Vector2.zero;
    }

    public static Vector2 GetProjection(Circle circle, Vector2 axis, bool isSkinWidth)
    {
        float centerProj = Vector2.Dot(circle.m_Position, axis);
        float radius = circle.GetBoundingSphereRadius(isSkinWidth);
        return new Vector2(centerProj - radius, centerProj + radius);
    }

    public static Vector2 GetProjection(Polygon poly,Vector2 axis,bool isSkinWidth)
    {
        Vector2 proj;
        var points = poly.GetBordersPoints(isSkinWidth);

        proj.x = Vector2.Dot(points[0],axis);
        proj.y = proj.x;

        for(int i = 1;i< points.Length;i++)
        {
            float newProj = Vector2.Dot(points[i],axis);
            if(newProj < proj.x)
            {
                proj.x = newProj;
            }
            else if(newProj > proj.y)
            {
                proj.y = newProj;
            }
        }
        return proj;
    }

    static bool DoProjectionsOverlap(Vector2 proj0, Vector2 proj1)
    {
	    bool result = true;

	    if ((proj0.y <= proj1.x) || (proj1.y <= proj0.x))
		    result = false;

	    return result;
    }

    static float CalculateOverlap(Vector2 proj0, Vector2 proj1)
    {
	    return Mathf.Max(0, Mathf.Min(proj0.y, proj1.y) - Mathf.Max(proj0.x, proj1.x));
    }

    static float CalculateMinDis(Vector2 proj0, Vector2 proj1)
    {
        return Mathf.Abs(Mathf.Max(proj0.x, proj1.x) - Mathf.Min(proj0.y, proj1.y));
    }

    #endregion

    static float Cross(Vector2 v0, Vector2 v1)
    {
        return v0.x * v1.y - v1.x * v0.y;
    }

    /// <summary>
    /// 两条线段是否相交
    /// </summary>
    /// <param name="a">线0的x</param>
    /// <param name="b">线0的y</param>
    /// <param name="c">线1的x</param>
    /// <param name="d">线1的y</param>
    /// <returns></returns>
    public static bool IsLineIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        //排斥试验
        if (Mathf.Max(a.x, b.x) < Mathf.Min(c.x, d.x))
        {
            return false;
        }

        if (Mathf.Min(a.x, b.x) > Mathf.Max(c.x, d.x))
        {
            return false;
        }

        if (Mathf.Max(a.y, b.y) < Mathf.Min(c.y, d.y))
        {
            return false;
        }

        if (Mathf.Min(a.y, b.y) > Mathf.Max(c.y, d.y))
        {
            return false;
        }

        //跨立试验
        Vector2 ca = a - c;
        Vector2 cd = d - c;
        Vector2 cb = b - c;
        Vector2 ac = c - a;
        Vector2 ab = b - a;
        Vector2 ad = d - a;

        float cross_ca_cd = Cross(ca, cd);
        float cross_cb_cd = Cross(cb, cd);
        float cross_ac_ab = Cross(ac, ab);
        float cross_ad_ab = Cross(ad, ab);

        return (cross_ca_cd * cross_cb_cd <= 0 && cross_ac_ab * cross_ad_ab <= 0);
    }

    public static bool IsLinePolygonIntersect(Vector2 start,Vector2 end,Polygon polygon)
    {
        var boards = polygon.GetBordersPoints(true);
        for (int i = 0; i < boards.Length; i++)
        {
            var p0 = boards[i];
            var p1 = boards[(i + 1) % boards.Length];
            if (IsLineIntersect(start, end, p0, p1))
            {
                return true;
            }
        }

        return false;
    }

    public static Vector2 LineIntersect(Vector2 a, Vector2 b,Vector2 c,Vector2 d)
    {
        Vector2 l1 = d - c;
        float p0 = Mathf.Abs(Cross(l1, a - c));
        float p1 = Mathf.Abs(Cross(l1, b - c));
        float temp = p0 / (p0 + p1);
        return a + (b - a) * temp;
    }

    public static Vector2[] OneLineShapeIntersect(Vector2 start,Vector2 end, Shape shape, bool isSkinWidth = true)
    {
        if(shape is Polygon polygon)
        {
            return OneLinePolygonIntersect(start, end, polygon, isSkinWidth);
        }
        else if(shape is Circle circle)
        {
            return OneLineCircleIntersect(start, end, circle, isSkinWidth);
        }

        return new Vector2[0];
    }

    public static Vector2[] OneLineCircleIntersect(Vector2 start,Vector2 end,Circle circle, bool isSkinWidth = true)
    {
        float radius = circle.GetBoundingSphereRadius(isSkinWidth);

        float A = (end.x - start.x) * (end.x - start.x) + (end.y - start.y) * (end.y - start.y);
        float B = 2 * ((end.x - start.x) * (start.x - circle.m_Position.x) + (end.y - start.y) * (start.y - circle.m_Position.y));
        float C = circle.m_Position.x * circle.m_Position.x + circle.m_Position.y * circle.m_Position.y + start.x * start.x
            + start.y * start.y - 2 * (circle.m_Position.x * start.x + circle.m_Position.y * start.y) - radius * radius;

        float T = B * B - 4 * A * C;
        if (T < 0)
        {
            return new Vector2[0];
        }
        else if (T == 0)
        {
            float U = -B / (2 * A);
            return new Vector2[1] { start + (end - start) * U };
        }
        else
        {
            float U0 = (-B + Mathf.Sqrt(T)) / (2 * A);
            float U1 = (-B - Mathf.Sqrt(T)) / (2 * A);
            if ((U0 < 0 || U0 > 1) && (U1 < 0 || U1 > 1))
            {
                return new Vector2[0];
            }
            else if(U0 >= 0 && U0 <= 1 && U1 >= 0 && U1 <= 1)
            {
                return new Vector2[2] { start + (end - start) * U0, start + (end - start) * U1 };
            }
            else if(U0 >= 0 && U0 <= 1)
            {
                return new Vector2[1] { start + (end - start) * U0 };
            }
            else if(U1 >= 0 && U1 <= 1)
            {
                return new Vector2[1] { start + (end - start) * U1 };
            }
            else
            {
                return new Vector2[0];
            }
        }

    }

    public static Vector2[] OneLinePolygonIntersect(Vector2 start, Vector2 end, Polygon polygon,bool isSkinWidth = true)
    {
        List<Vector2> intersectPointList = new List<Vector2>();
        var boards = polygon.GetBordersPoints(isSkinWidth);
        for(int i =0;i< boards.Length;i++)
        {
            var p0 = boards[i];
            var p1 = boards[(i + 1)% boards.Length];
            if(IsLineIntersect(start,end,p0,p1))
            {
                intersectPointList.Add(LineIntersect(start, end, p0, p1));
            }
        }

        intersectPointList.Sort((Vector2 a, Vector2 b) =>
        {
            var dis0 = Vector2.Distance(a, start);
            var dis1 = Vector2.Distance(b, start);
            if (dis0 < dis1) return -1;
            else if (dis0 > dis1) return 1;
            else return 0;
        });

        return intersectPointList.ToArray();
    }

    public static (bool, Vector2) CaculateShapeIntersectTangent(Vector2 intersect,Shape shape, bool isSkinWidth = true)
    {
        if (shape is Polygon polygon)
        {
            return CaculatePolygonIntersectTangent(intersect, polygon, isSkinWidth);
        }
        else if (shape is Circle circle)
        {
            return CaculateCircleIntersectTangent(intersect, circle, isSkinWidth);
        }

        return (false, Vector2.zero);
    }

    public static (bool,Vector2) CaculatePolygonIntersectTangent(Vector2 intersect,Polygon polygon, bool isSkinWidth = true)
    {
        var boards = polygon.GetBordersPoints(isSkinWidth);
        for (int i = 0; i < boards.Length; i++)
        {
            var start = boards[i];
            var end = boards[(i + 1) % boards.Length];
            if (IsPointInLine(start, end, intersect))
            {
                return (true,(end - start).normalized);
            }
        }

        return (false, Vector2.zero);
    }

    public static (bool,Vector2) CaculateCircleIntersectTangent(Vector2 intersect, Circle circle, bool isSkinWidth = true)
    {
        float radius = circle.GetBoundingSphereRadius(isSkinWidth);
        if(Mathf.Abs(Vector2.Distance(circle.m_Position,intersect) - radius)< 0.001f)
        {
            var line = intersect - circle.m_Position;
            return (true, new Vector2(-line.y, line.x).normalized);
        }
        return (false, Vector2.zero);
    }

    public static Vector2[] LinePolygonIntersect(Vector2[] linePoints,Polygon[] polygons)
    {
        List<Vector2> newLinePoints = new List<Vector2>();
        List<Vector2> removePoints = new List<Vector2>();
        bool isStartInPolygon = false;
        Vector2 frontPoint = Vector2.zero;
        for(int i = 0;i< linePoints.Length;i++)
        {
            newLinePoints.Add(linePoints[i]);
        }

        for (int i = 0; i < linePoints.Length -1; i++)
        {
            var start = linePoints[i];
            var end = linePoints[i + 1];
            List<Polygon> intersecetPolygons = new List<Polygon>();
            for (int j = 0; j < polygons.Length; j++)
            {
                var polygon = polygons[j];
                if (IsLinePolygonIntersect(start, end, polygon))
                {
                    intersecetPolygons.Add(polygon);
                }
            }

            intersecetPolygons.Sort((Polygon a, Polygon b) =>
            {
                var dis0 = Vector2.Distance(a.m_Position, start);
                var dis1 = Vector2.Distance(b.m_Position, start);
                if (dis0 < dis1) return -1;
                else if (dis0 > dis1) return 1;
                else return 0;
            });

            List<Vector2> addPoints = new List<Vector2>();
            for(int j =0;j< intersecetPolygons.Count;j++)
            {
                var polygon = intersecetPolygons[j];
                var intersectPoints = OneLinePolygonIntersect(start, end, polygon);
                if(intersectPoints.Length == 1)
                {
                    if(isStartInPolygon)
                    {
                        isStartInPolygon = false;
                        var edgePoints = CaculateEdgePoints(frontPoint, intersectPoints[0], polygon);
                        removePoints.Add(start);
                        addPoints.AddRange(edgePoints);
                    }
                    else
                    {
                        isStartInPolygon = true;
                        frontPoint = intersectPoints[0];
                    }
                }
                else if(intersectPoints.Length == 2)
                {
                    var edgePoints = CaculateEdgePoints(intersectPoints[0], intersectPoints[1], polygon);
                    addPoints.AddRange(edgePoints);
                }
            }

            int startIndex = newLinePoints.IndexOf(start);
            newLinePoints.InsertRange(startIndex + 1, addPoints);
        }

        for(int i = 0;i< removePoints.Count;i++)
        {
            var removePoint = removePoints[i];
            if(newLinePoints.Contains(removePoint))
            {
                newLinePoints.Remove(removePoint);
            }
        }

        return newLinePoints.ToArray();
    }

    static Vector2[] CaculateEdgePoints(Vector2 p0,Vector2 p1, Polygon polygon)
    {
        List<Vector2> leftPoints = new List<Vector2>();
        List<Vector2> rightPoints = new List<Vector2>();

        var boards = polygon.GetBordersPoints(true);
        int p0Start = 0, p0End = 0, p1Start = 0, p1End = 0; 
        for(int i =0;i< boards.Length;i++)
        {
            var start = boards[i];
            var end = boards[(i + 1) % boards.Length];
            if(IsPointInLine(start,end,p0))
            {
                p0Start = i;
                p0End = (i + 1) % boards.Length;
            }

            if(IsPointInLine(start,end,p1))
            {
                p1Start = i;
                p1End = (i + 1) % boards.Length;
            }
        }

        if(p0Start == p1Start && p0End == p1End)
        {
            return new Vector2[2] { p0, p1 };
        }

        float distanceLeft = 0;
        leftPoints.Add(p0);
        for (int i = p0End; i < p0End + boards.Length; i++)
        {
            var point = boards[i % boards.Length];
            leftPoints.Add(point);
            distanceLeft += Vector2.Distance(leftPoints[leftPoints.Count - 1], leftPoints[leftPoints.Count - 2]);
            if (i == p1Start)
            {
                leftPoints.Add(p1);
                distanceLeft += Vector2.Distance(leftPoints[leftPoints.Count - 1], leftPoints[leftPoints.Count - 2]);
                break;
            }
        }

        float distanceRight = 0;
        rightPoints.Add(p0);
        for(int i = p0Start + boards.Length; i > p0Start;i--)
        {
            var point = boards[i % boards.Length];
            rightPoints.Add(point);
            distanceRight += Vector2.Distance(rightPoints[rightPoints.Count - 1], rightPoints[rightPoints.Count - 2]);
            if (i % boards.Length == p1End)
            {
                rightPoints.Add(p1);
                distanceRight += Vector2.Distance(rightPoints[rightPoints.Count - 1], rightPoints[rightPoints.Count - 2]);
                break;
            }
        }

        return distanceLeft < distanceRight ? leftPoints.ToArray() : rightPoints.ToArray();


    }

    public static bool IsPointInLine(Vector2 start,Vector2 end, Vector2 point)
    {
        if (point == start || point == end) return true;
        var dir0 = (point - start).normalized;
        var dir1 = (point - end).normalized;
        return dir0 == -dir1;
    }


    public static Vector2 RotateAroundPoint(Vector2 aroundPoint,Vector2 targetPoint,float angle)
    {
        Vector2 p0 = targetPoint - aroundPoint;
        Vector2 p1 = new Vector2(Mathf.Cos(angle) * p0.x - Mathf.Sin(angle) * p0.y,
            Mathf.Sin(angle) * p0.x + Mathf.Cos(angle) * p0.y);

        return p1 + aroundPoint;
    }

    public static float Vector2Rangle(Vector2 a,Vector2 b)
    {
        return Mathf.Acos(a.x * b.x + a.y * b.y) * (Cross(a,b)> 0 ? 1 : -1);
    }

    #region ConvexHull
    //凸包生成

    public static Vector2[] GrahamScan(Vector2[] points)
    {
        Vector2 p0 = Vector2.zero; ;
        List<Vector2> pointList = new List<Vector2>();
        float minY = float.MaxValue;
        for(int i =0;i < points.Length;i++)
        {
            if (points[i].y < minY)
            {
                p0 = points[i];
                minY = points[i].y;
            }
            pointList.Add(points[i]);
        }

        pointList.Sort(
           (Vector2 a, Vector2 b) =>
           {
               if (a == p0)
               {
                   return -1;
               }
               if( b == p0)
               {
                   return 1;
               }

               Vector2 dir0 = (a - p0).normalized;
               Vector2 dir1 = (b - p0).normalized;
               Vector2 xAxis = new Vector2(1, 0);

               float dot0 = Vector2.Dot(xAxis, dir0);
               float dot1 = Vector2.Dot(xAxis, dir1);

               return dot0 >= dot1 ? -1 : 1;
           }
        );

        
        for(int i = pointList.Count - 2; i > 0; i--)
        {
            var p1 = pointList[i + 1];
            var p2 = pointList[i];

            var dir1 = p1 - p0;
            var dir2 = p2 - p0;
            if(dir1.normalized == dir2.normalized)
            {
                if (dir1.magnitude > dir2.magnitude)
                {
                    pointList.RemoveAt(i);
                }
                else
                {
                    pointList.RemoveAt(i + 1);
                }
            }

        }
        if(pointList.Count < 3)
        {
            return new Vector2[0];
        }

        Stack<Vector2> pointStack = new Stack<Vector2>();
        Stack<Vector2> prePointStack = new Stack<Vector2>();
        pointStack.Push(p0);
        pointStack.Push(pointList[1]);
        prePointStack.Push(p0);

        for (int i = 2;i < pointList.Count;i++)
        {
            var prePoint = prePointStack.Peek();
            var p1 = pointStack.Peek();
            var p2 = pointList[i];
            var dir1 = p1 - prePoint;
            var dir2 = p2 - prePoint;
            float cross = Cross(dir1, dir2);

            if(cross > 0)
            {
                pointStack.Push(p2);
                prePointStack.Push(p1);
            }
            else
            {
                while (prePointStack.Count > 0)
                {
                    pointStack.Pop();
                    prePointStack.Pop();
                    p1 = pointStack.Peek();
                    prePoint = prePointStack.Peek();

                    dir1 = p1 - prePoint;
                    dir2 = p2 - prePoint;
                    cross = Cross(dir1, dir2);

                    if (cross > 0)
                    {
                        pointStack.Push(p2);
                        prePointStack.Push(p1);
                        break;
                    }
                }
            }
        }

        pointList.Clear();
        while(pointStack.Count > 0)
        {
            pointList.Add(pointStack.Pop());
        }

        return pointList.ToArray();
      
    }

    public static float TriangleArea(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float area = 0;
        area = p0.x * p1.y + p1.x * p2.y + p2.x * p0.y - p1.x * p0.y - p2.x * p1.y - p0.x * p2.y;
        return area / 2;
    }

    public static (Vector2,float)CaculatePointsBoundingSphere2D(Vector2[] points)
    {
        if(points.Length < 2)
        {
            return (Vector2.zero, 0);
        }

        Vector2[] convexHullPoints = GrahamScan(points);

        if(convexHullPoints.Length < 2)
        {
            return (Vector2.zero, 0);
        }

        float sum_x, sum_y, sum_area, area;
        sum_x = sum_y = sum_area = 0;
        var p0 = convexHullPoints[0];
        var p1 = convexHullPoints[1];

        for(int i =2;i< convexHullPoints.Length;i++)
        {
            var p2 = convexHullPoints[i];
            area = TriangleArea(p0, p1, p2);
            sum_area += area;
            sum_x += (p0.x + p1.x + p2.x) * area;
            sum_y += (p0.y + p1.y + p2.y) * area;
            p1 = p2;
        }

        Vector2 center = new Vector2(sum_x / sum_area / 3, sum_y / sum_area / 3);

        float radius = 0;
        for(int i =0;i< convexHullPoints.Length;i++)
        {
            var dis = Vector2.Distance(center, convexHullPoints[i]);
            if(dis > radius)
            {
                radius = dis;
            }
        }

        return (center, radius);


    }


    #endregion

}

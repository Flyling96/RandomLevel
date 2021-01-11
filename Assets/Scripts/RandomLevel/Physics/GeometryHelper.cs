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
        m_Pole = Random.Range(0, 500) % 2;
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
        m_Pole = Random.Range(0, 500) % 2;
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


public static class GeometryHelper 
{
    public static float AttractStrength = 0.2f;

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
        bool isRepulsive = c0.m_Pole == 0 || c1.m_Pole == 0;
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
        bool isRepulsive = p0.m_Pole == 0 || c0.m_Pole == 0;

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
        bool isRepulsive = p0.m_Pole == 0 || p1.m_Pole == 0;

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
}

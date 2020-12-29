using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    public Vector2 m_Position;

    public Vector3 m_Normal;

    public Vector2 m_Center;

    public Vector2[] m_Borders;

    public float m_BoundingSphereRadius;

    public Polygon(Vector2 center,Vector2[] borders,Vector2 position,Vector3 normal)
    {
        m_Center = center;
        m_Borders = borders;
        m_Position = position;
        m_Normal = normal;
        m_BoundingSphereRadius = GetBoundingSphereRadius();
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

    public Vector2[] GetBordersPoints()
    {
        Vector2[] points = new Vector2[m_Borders.Length];
        for(int i =0;i<points.Length;i++)
        {
            points[i] = m_Position + m_Borders[i];
        }
        return points;
    }

    public float GetBoundingSphereRadius()
    {
        float radius = 0;
        for(int i =0;i<m_Borders.Length;i++)
        {
            var length = (m_Center - m_Borders[i]).magnitude;
            if(length > radius)
            {
                radius = length;
            }
        }
        return radius;
    }
}


public class SeparatingAxisAlgorithm 
{
    public bool SeparatingAxis(Polygon p0,Polygon p1)
    {
        if(p0.m_Borders.Length < 3 || p1.m_Borders.Length < 3)
        {
            return false;
        }

        float distance = (p0.m_Position - p1.m_Position).magnitude;
        if(distance > p0.m_BoundingSphereRadius + p1.m_BoundingSphereRadius)
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
            p0Proj = GetProjection(p0, axes0[i]);
            p1Proj = GetProjection(p1, axes0[i]);
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
            p0Proj = GetProjection(p0, axes1[i]);
            p1Proj = GetProjection(p1, axes1[i]);
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

        Vector2 moveDir = projValue * projAxis / 2;
        p0.Move(-moveDir);
        p1.Move(moveDir);
        return true;
    }

    public Vector2 GetProjection(Polygon poly,Vector2 axis)
    {
        Vector2 proj;
        var points = poly.GetBordersPoints();

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

    bool DoProjectionsOverlap(Vector2 proj0, Vector2 proj1)
    {
	    bool result = true;

	    if ((proj0.y <= proj1.x) || (proj1.y <= proj0.x))
		    result = false;

	    return result;
    }

    float CalculateOverlap(Vector2 proj0, Vector2 proj1)
    {
	    return Mathf.Max(0, Mathf.Min(proj0.y, proj1.y) - Mathf.Max(proj0.x, proj1.x));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    [System.Serializable]
    public class Route : ISerializationCallbackReceiver
    {
        public Vector3 m_Position;

        public Quaternion m_Rotation = Quaternion.identity;

        private Matrix4x4 m_TRS;

        public void UpdateTransform(Vector3 pos,Quaternion rot)
        {
            bool isChange = false;
            if(m_Position != pos)
            {
                m_Position = pos;
                isChange = true;
            }

            if(m_Rotation != rot)
            {
                m_Rotation = rot;
                isChange = true;
            }

            if(isChange)
            {
                m_TRS = Matrix4x4.TRS(m_Position, m_Rotation, Vector3.one);
            }
        }

        public List<RoutePoint> m_Points = new List<RoutePoint>();


        public Route(Vector3 pos,Quaternion rot)
        {
            m_Position = pos;
            m_Rotation = rot;
        }

        public void AddPoints(Vector3 pos)
        {
            RoutePoint newPoint = new RoutePoint();
            newPoint.m_LocalPos = Matrix4x4.Inverse(m_TRS).MultiplyPoint(pos);
            m_Points.Add(newPoint);
        }

        public void DeletePoints(RoutePoint target)
        {
            if(!m_Points.Contains(target))
            {
                return;
            }

            target.OnDelete();
            m_Points.Remove(target);
        }

        public void Connect(RoutePoint p0,RoutePoint p1)
        {
            p0.OnConnect(p1);
            p1.OnConnect(p0);
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            m_TRS = Matrix4x4.TRS(m_Position, m_Rotation, Vector3.zero);
        }

        public Mesh ConvertToMesh()
        {
            if(m_Points.Count < 2)
            {
                return null;
            }

            Mesh mesh = new Mesh();

            for(int i =0; i < m_Points.Count - 1;i++)
            {

            }

            return mesh; 
        }
    }
}

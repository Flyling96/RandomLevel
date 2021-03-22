using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.Route
{
    [System.Serializable]
    public class RoutePoint 
    {
        public Vector3 m_LocalPos;

        public List<RoutePoint> m_NeighborPoints = new List<RoutePoint>();


        public void OnConnect(RoutePoint neighbor)
        {

            if(!m_NeighborPoints.Contains(neighbor))
            {
                m_NeighborPoints.Add(neighbor);
            }


        }

        public void OnDisConnect(RoutePoint neighbor)
        {
            if(!m_NeighborPoints.Contains(neighbor))
            {
                return;
            }

            m_NeighborPoints.Remove(neighbor);

        }


        public void OnDelete()
        {
            for(int i = 0; i < m_NeighborPoints.Count;i++)
            {
                var neightbor = m_NeighborPoints[i];
                neightbor.OnDisConnect(this);
            }
        }




    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DragonSlay.Route
{
    [System.Serializable]
    public class RoutePoint : ISerializationCallbackReceiver
    {
        public ulong m_UID;

        public Vector3 m_LocalPos;

        public bool m_IsMain = false;

        //public List<RoutePoint> m_NeighborPoints = new List<RoutePoint>();
        [NonSerialized]
        public RoutePoint m_PrePoint = null;
        [SerializeField]
        private ulong m_SerializedPrePointUID;

        public ulong SerializedPrePointUID { get { return m_SerializedPrePointUID; }}

        [NonSerialized]
        public RoutePoint m_ProPoint = null;
        [SerializeField]
        private ulong m_SerializedProPointUID;

        public ulong SerializedProPointUID { get { return m_SerializedProPointUID; } }

        [NonSerialized]
        public List<RoutePoint> m_ForkPoints = new List<RoutePoint>();

        [SerializeField]
        private List<ulong> m_SerializedForkPointUIDs = new List<ulong>();

        public List<ulong> SerializedForkPointUIDs { get { return m_SerializedForkPointUIDs; } }

        [NonSerialized]
        public RouteSubMesh[] m_BelongSubMeshes = new RouteSubMesh[2];

        public RoutePoint(Vector3 localPos)
        {
            m_LocalPos = localPos;
        }

        public void AddSubMesh(RouteSubMesh subMesh)
        {
            if(m_BelongSubMeshes[0] == null)
            {
                m_BelongSubMeshes[0] = subMesh;
            }
            else if(m_BelongSubMeshes[1] == null)
            {
                m_BelongSubMeshes[1] = subMesh;
            }
        }

        public void ClearSubMesh()
        {
            if(m_BelongSubMeshes == null)
            {
                m_BelongSubMeshes = new RouteSubMesh[2];
            }

            m_BelongSubMeshes[0] = null;
            m_BelongSubMeshes[1] = null;
        }

        public RouteSubMesh GetOtherSubMesh(RouteSubMesh subMesh)
        {
            if (m_BelongSubMeshes[0] == subMesh)
            {
                return m_BelongSubMeshes[1];
            }
            else
            {
                return m_BelongSubMeshes[0];
            }
        }

        public bool IsStraight
        {
            get
            {
                return !(IsFork | IsTurn);
            }
        }

        public bool IsFork
        {
            get
            {
                return m_ForkPoints.Count > 0;
            }
        }

        public bool IsGate
        {
            get
            {
                return m_PrePoint == null || m_ProPoint == null;
            }
        }

        public bool IsTurn
        {
            get
            {
                if (IsGate || IsFork) return false;
                var dir0 = m_LocalPos - m_PrePoint.m_LocalPos;
                var dir1 = m_ProPoint.m_LocalPos - m_LocalPos;
                return Mathf.Abs(Vector3.Dot(dir0, dir1) - dir0.magnitude * dir1.magnitude) > 0.01f;
            }
        }


        public void OnConnect(RoutePoint neighbor,bool isPrePoint = false)
        {
            if(m_PrePoint == null && m_ProPoint == null)
            {
                if(isPrePoint)
                {
                    m_PrePoint = neighbor;
                }
                else
                {
                    m_ProPoint = neighbor;
                }
            }
            else if(m_PrePoint == null)
            {
                m_PrePoint = neighbor;
            }
            else if(m_ProPoint == null)
            {
                m_ProPoint = neighbor;
            }
            else
            {
                if(!m_ForkPoints.Contains(neighbor))
                {
                    m_ForkPoints.Add(neighbor);
                }
            }

        }

        public void OnDisconnect(RoutePoint neighbor)
        {
            if(m_PrePoint == neighbor)
            {
                m_PrePoint = null;
            }
            else if(m_ProPoint == neighbor)
            {
                m_ProPoint = null;
            }
            else
            {
                if(m_ForkPoints.Contains(neighbor))
                {
                    m_ForkPoints.Remove(neighbor);
                }
            }


        }


        public void OnDelete()
        {
            if(m_PrePoint != null)
            {
                m_PrePoint.OnDisconnect(this);
            }

            if(m_ProPoint != null)
            {
                m_ProPoint.OnDisconnect(this);
            }

            for(int i = 0; i < m_ForkPoints.Count;i++)
            {
                var neightbor = m_ForkPoints[i];
                neightbor.OnDisconnect(this);
            }
        }

        public void OnBeforeSerialize()
        {
            if(m_PrePoint != null)
            {
                m_SerializedPrePointUID = m_PrePoint.m_UID;
            }
            else
            {
                m_SerializedPrePointUID = 0;
            }

            if(m_ProPoint != null)
            {
                m_SerializedProPointUID = m_ProPoint.m_UID;
            }
            else
            {
                m_SerializedProPointUID = 0;
            }

            if(m_SerializedForkPointUIDs == null)
            {
                m_SerializedForkPointUIDs = new List<ulong>();
            }

            m_SerializedForkPointUIDs.Clear();
            if (m_ForkPoints != null)
            {
                for (int i = 0; i < m_ForkPoints.Count; i++)
                {
                    var forkPoint = m_ForkPoints[i];
                    if (forkPoint != null)
                    {
                        m_SerializedForkPointUIDs.Add(forkPoint.m_UID);
                    }
                    else
                    {
                        m_SerializedForkPointUIDs.Add(0);
                    }
                }
            }
        }

        public void OnAfterDeserialize()
        {
            if(m_ForkPoints == null)
            {
                m_ForkPoints = new List<RoutePoint>();
            }

            if(m_BelongSubMeshes == null)
            {
                m_BelongSubMeshes = new RouteSubMesh[2];
            }
        }
    }
}

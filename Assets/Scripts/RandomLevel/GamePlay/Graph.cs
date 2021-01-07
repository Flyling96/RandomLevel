using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Gameplay
{
    public class Graph
    {
        public struct Edge
        {
            public int m_Point0;
            public int m_Point1;
            public int m_Distance;

            public bool IsEqual(int point0,int point1)
            {
                return m_Point0 == point0 && m_Point1 == point1 ||
                       m_Point0 == point1 && m_Point1 == point0;
            }

            public int IsConnect(int point)
            {
                if(m_Point0 == point)
                {
                    return m_Point1;
                }
                else if(m_Point1 == point)
                {
                    return m_Point0;
                }
                else
                {
                    return -1;
                }
            }
        }

        HashSet<Edge> m_EdgeSet = new HashSet<Edge>();

        public void AddEdge(Edge edge)
        {
            m_EdgeSet.Add(edge);
        }

        public Dictionary<int,int> GetPointDepth(int start)
        {
            Dictionary<int, int> roomDepthDic = new Dictionary<int, int>();
            HashSet<int> findNext = new HashSet<int>();
            HashSet<int> alreadyFind = new HashSet<int>();
            findNext.Add(start);
            alreadyFind.Add(start);
            roomDepthDic.Add(start, 0);
            int depth = 1;

            while (findNext.Count > 0)
            {
                HashSet<int> nextSet = new HashSet<int>();
                foreach (var id in findNext)
                {
                    foreach(var edge in m_EdgeSet)
                    {
                        int connectId = edge.IsConnect(id);
                        if(connectId > -1 && !alreadyFind.Contains(connectId))
                        {
                            roomDepthDic.Add(connectId, depth);
                            alreadyFind.Add(connectId);
                            nextSet.Add(connectId);
                        }
                    }
                }
                depth++;
                findNext = nextSet;
            }
            return roomDepthDic;
        }



    }
}

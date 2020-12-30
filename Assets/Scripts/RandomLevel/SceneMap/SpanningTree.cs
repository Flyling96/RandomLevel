using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSlay.RandomLevel
{
    public class SpanningTree
    {
        public static List<UEdge2D> Kruskal(Dictionary<int, UVertex2D> vertexs, List<UEdge2D> uEdge2Ds)
        {
            Graph g = new Graph(vertexs.Count);
            for (int i = 0; i < uEdge2Ds.Count; i++)
            {
                g.AddEdge(uEdge2Ds[i].Points[0].Id, uEdge2Ds[i].Points[1].Id, uEdge2Ds[i].Distance);
            }
            var kruskal = g.Kruskal();
            List<UEdge2D> result = new List<UEdge2D>();
            for (int i = 0; i < kruskal.Count; i++)
            {
                if (!vertexs.ContainsKey(kruskal[i].Begin) || !vertexs.ContainsKey(kruskal[i].End))
                {
                    continue;
                }
                result.Add(new UEdge2D(vertexs[kruskal[i].Begin], vertexs[kruskal[i].End]));
            }
            UnityEngine.Debug.Log("kruskal edge count:" + result.Count);
            return result;
        }

        class Edge
        {
            public Edge(int begin, int end, float weight)
            {
                this.Begin = begin;
                this.End = end;
                this.Weight = weight;
            }

            public int Begin { get; private set; }
            public int End { get; private set; }
            public float Weight { get; private set; }

            public override string ToString()
            {
                return string.Format(
                  "Begin[{0}], End[{1}], Weight[{2}]",
                  Begin, End, Weight);
            }
        }

        class Subset
        {
            public int Parent { get; set; }
            public int Rank { get; set; }
        }

        class Graph
        {
            private Dictionary<int, List<Edge>> _adjacentEdges
              = new Dictionary<int, List<Edge>>();

            public Graph(int vertexCount)
            {
                this.VertexCount = vertexCount;
            }

            public int VertexCount { get; private set; }

            public IEnumerable<int> Vertices { get { return _adjacentEdges.Keys; } }

            public IEnumerable<Edge> Edges
            {
                get { return _adjacentEdges.Values.SelectMany(e => e); }
            }

            public int EdgeCount { get { return this.Edges.Count(); } }

            public void AddEdge(int begin, int end, float weight)
            {
                if (!_adjacentEdges.ContainsKey(begin))
                {
                    var edges = new List<Edge>();
                    _adjacentEdges.Add(begin, edges);
                }

                _adjacentEdges[begin].Add(new Edge(begin, end, weight));
            }

            private int Find(Subset[] subsets, int i)
            {
                // find root and make root as parent of i (path compression)
                if (subsets[i].Parent != i)
                    subsets[i].Parent = Find(subsets, subsets[i].Parent);

                return subsets[i].Parent;
            }

            private void Union(Subset[] subsets, int x, int y)
            {
                int xroot = Find(subsets, x);
                int yroot = Find(subsets, y);

                // Attach smaller rank tree under root of high rank tree
                // (Union by Rank)
                if (subsets[xroot].Rank < subsets[yroot].Rank)
                    subsets[xroot].Parent = yroot;
                else if (subsets[xroot].Rank > subsets[yroot].Rank)
                    subsets[yroot].Parent = xroot;

                // If ranks are same, then make one as root and increment
                // its rank by one
                else
                {
                    subsets[yroot].Parent = xroot;
                    subsets[xroot].Rank++;
                }
            }

            public bool HasCycle()
            {
                Subset[] subsets = new Subset[VertexCount];
                for (int i = 0; i < subsets.Length; i++)
                {
                    subsets[i] = new Subset();
                    subsets[i].Parent = i;
                    subsets[i].Rank = 0;
                }

                // Iterate through all edges of graph, find subset of both
                // vertices of every edge, if both subsets are same, 
                // then there is cycle in graph.
                foreach (var edge in this.Edges)
                {
                    int x = Find(subsets, edge.Begin);
                    int y = Find(subsets, edge.End);

                    if (x == y)
                    {
                        return true;
                    }

                    Union(subsets, x, y);
                }

                return false;
            }

            public List<Edge> Kruskal()
            {
                // This will store the resultant MST
                List<Edge> mst = new List<Edge>();

                // Step 1: Sort all the edges in non-decreasing order of their weight
                // If we are not allowed to change the given graph, we can create a copy of
                // array of edges
                var sortedEdges = this.Edges.OrderBy(t => t.Weight);
                var enumerator = sortedEdges.GetEnumerator();

                // Allocate memory for creating V ssubsets
                // Create V subsets with single elements
                Subset[] subsets = new Subset[VertexCount];
                for (int i = 0; i < subsets.Length; i++)
                {
                    subsets[i] = new Subset();
                    subsets[i].Parent = i;
                    subsets[i].Rank = 0;
                }

                // Number of edges to be taken is equal to V-1
                int e = 0;
                while (e < VertexCount - 1 && enumerator.MoveNext())
                {
                    // Step 2: Pick the smallest edge. And increment the index
                    // for next iteration
                    Edge nextEdge;

                    nextEdge = enumerator.Current;

                    int x = Find(subsets, nextEdge.Begin);
                    int y = Find(subsets, nextEdge.End);

                    // If including this edge does't cause cycle, include it
                    // in result and increment the index of result for next edge
                    if (x != y)
                    {
                        mst.Add(nextEdge);
                        e++;
                        Union(subsets, x, y);
                    }
                    else
                    {
                        // Else discard the nextEdge
                    }
                }

                return mst;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using EditorCools;

namespace Pathfinding
{
    public class AStarPathfinder : Singleton<AStarPathfinder>
    {
        [Header("Testing")]
        public PointNode startNode;
        public PointNode endNode;
        public GraphManager graphManager;

        [Button]
        private void Find()
        {
            endNode = graphManager.points[Random.Range(0, graphManager.points.Count)];
            if (endNode == null || startNode == endNode) return;

            var nodes = FindPath(startNode, endNode);
            Debug.Log(nodes != null ? "Path found" : "Path not found");
            //if (nodes != null) PathDrawer.Instance.DrawPath(nodes);
        }

        public List<PointNode> FindPath(PointNode start, PointNode goal)
        {
            if (start == null || goal == null) return null;

            var cameFrom = new Dictionary<PointNode, PointNode>();
            var gScore = new Dictionary<PointNode, float> { [start] = 0 };
            var openSet = new PriorityQueue<PointNode>();
            openSet.Enqueue(start, 0);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();
                if (current == goal) return ReconstructPath(cameFrom, goal);

                foreach (var neighbor in current.neighbors)
                {
                    if (neighbor == null) continue;
                    float tentativeG = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);
                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float priority = tentativeG + Heuristic(neighbor, goal);
                        openSet.Enqueue(neighbor, priority);
                    }
                }
            }
            return null;
        }

        private float Heuristic(PointNode a, PointNode b) => Vector3.Distance(a.transform.position, b.transform.position);

        private List<PointNode> ReconstructPath(Dictionary<PointNode, PointNode> cameFrom, PointNode current)
        {
            var path = new List<PointNode> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}

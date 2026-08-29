using EditorCools;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class GraphManager : Singleton<GraphManager>
    {
        public List<PointNode> points;
        public float connectionDistance = 5f;

        public PointNode GetNearestNode(Transform obj)
        {
            if (points == null || points.Count == 0) return null;
            PointNode nearest = null;
            float minDistance = Mathf.Infinity;
            var objPos = obj.position;
            foreach (var point in points)
            {
                if (point == null) continue;
                float dist = Vector3.Distance(objPos, point.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = point;
                }
            }
            return nearest;
        }

        [SerializeField] PointNode nodePrefab;
        public Vector3 startPositionNode;
        public Vector3 endPositionNode;
        public PointNode nodeStartConnect;
        public PointNode nodeEndConnect;

        [Button]
        private void ConnectNode()
        {
            if (nodeStartConnect == null || nodeEndConnect == null) return;
            if (!nodeStartConnect.neighbors.Contains(nodeEndConnect))
                nodeStartConnect.neighbors.Add(nodeEndConnect);
            if (!nodeEndConnect.neighbors.Contains(nodeStartConnect))
                nodeEndConnect.neighbors.Add(nodeStartConnect);
        }

        [Button]
        private void AutoCreateNode()
        {
            float minX = Mathf.Min(startPositionNode.x, endPositionNode.x);
            float maxX = Mathf.Max(startPositionNode.x, endPositionNode.x);
            float minZ = Mathf.Min(startPositionNode.z, endPositionNode.z);
            float maxZ = Mathf.Max(startPositionNode.z, endPositionNode.z);
            float y = startPositionNode.y;

            for (float x = minX; x <= maxX; x += connectionDistance)
                for (float z = minZ; z <= maxZ; z += connectionDistance)
                {
                    var spawnPos = new Vector3(x, y, z);
                    var nodeObj = Instantiate(nodePrefab, spawnPos, Quaternion.identity, transform);
                    nodeObj.transform.localPosition = spawnPos;
                }
        }

        [Button]
        private void GetAllNode()
        {
            points = new List<PointNode>();
            foreach (Transform child in transform)
            {
                var p = child.GetComponent<PointNode>();
                if (p != null) points.Add(p);
            }
        }

        [Button]
        private void BuildGraph()
        {
            if (points == null || points.Count == 0) return;
            foreach (var point in points)
            {
                if (point == null) continue;
                point.neighbors.Clear();
                foreach (var other in points)
                {
                    if (other == null || point == other) continue;
                    if (Vector3.Distance(point.transform.position, other.transform.position) < connectionDistance)
                        point.neighbors.Add(other);
                }
            }
        }

        [Button]
        private void RemoveDuplicateNodes()
        {
            var seenPositions = new HashSet<Vector3>();
            var uniquePoints = new List<PointNode>();
            foreach (var point in points)
            {
                if (point == null) continue;
                if (seenPositions.Contains(point.transform.position))
                {
                    Debug.Log($"🗑 Xóa node trùng: {point.name}");
                    DestroyImmediate(point.gameObject);
                }
                else
                {
                    seenPositions.Add(point.transform.position);
                    uniquePoints.Add(point);
                }
            }
            points = uniquePoints;
        }

        [Button]
        private void Rename()
        {
            for (int i = 0; i < points.Count; i++)
                if (points[i] != null)
                    points[i].name = $"Waypoint {i}";
        }

        [Button]
        private void GetMissingNeighbors()
        {
            foreach (var point in points)
                if (point.neighbors.Exists(n => n == null))
                    Debug.Log(point.name);
        }

        [Button]
        private void RemoveAllNodeMissing()
        {
            foreach (var point in points)
                point.neighbors.RemoveAll(n => n == null);
        }
    }
}

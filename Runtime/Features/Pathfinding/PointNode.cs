using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class PointNode : MonoBehaviour
    {
        public string[] targets;
        public List<PointNode> neighbors = new();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.2f);
            if (neighbors == null) return;
            foreach (var neighbor in neighbors)
                if (neighbor != null)
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }

        public Vector3 GetPosition() => transform.position;
    }
}

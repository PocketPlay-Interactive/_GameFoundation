using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class PathDrawer : Singleton<PathDrawer>
    {
        public bool IsDraw = false;
        public LineRenderer lineRenderer;
        private Material lineMaterial;
        private readonly List<Vector3> positions = new();
        public List<Vector3> GetAllPath() => positions;
        public int GetPathNumber() => positions.Count;

        public void DrawPath(List<PointNode> path)
        {
            positions.Clear();
            IsDraw = true;
            lineMaterial = lineRenderer.material;
            if (path == null || path.Count == 0)
            {
                lineRenderer.positionCount = 0;
                return;
            }
            lineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                lineRenderer.SetPosition(i, path[i].transform.position);
                positions.Add(path[i].transform.position);
            }
        }

        public void RemovePath()
        {
            IsDraw = false;
            lineRenderer.positionCount = 0;
        }

        public float scrollSpeed = 1f;

        void Update()
        {
            if (!IsDraw) return;
            if (lineRenderer != null && lineRenderer.material != null)
            {
                float offset = Time.time * scrollSpeed;
                lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
            }
        }
    }
}

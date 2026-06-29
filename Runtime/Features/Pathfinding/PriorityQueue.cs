using System.Collections.Generic;
using System.Linq;

namespace Pathfinding
{
    public class PriorityQueue<T>
    {
        private readonly List<KeyValuePair<T, float>> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority) => elements.Add(new(item, priority));

        public T Dequeue()
        {
            int bestIndex = 0;
            float bestPriority = elements[0].Value;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].Value < bestPriority)
                {
                    bestPriority = elements[i].Value;
                    bestIndex = i;
                }
            var bestItem = elements[bestIndex].Key;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public bool Contains(T item) => elements.Any(e => EqualityComparer<T>.Default.Equals(e.Key, item));
    }
}

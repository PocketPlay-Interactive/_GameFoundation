using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation.Patterns
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Queue<T> pool = new();

        public ObjectPool(T prefab, int initialSize = 10)
        {
            this.prefab = prefab;
            for (int i = 0; i < initialSize; i++)
            {
                var obj = Object.Instantiate(prefab);
                obj.gameObject.Hide(); // pooled objects start inactive
                pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            while (pool.Count > 0 && (Object)pool.Peek() == null) pool.Dequeue(); // drop destroyed entries (Unity null check)
            if (pool.Count == 0)
                pool.Enqueue(Object.Instantiate(prefab));
            var obj = pool.Dequeue();
            obj.gameObject.Show();
            return obj;
        }

        public void Release(T obj)
        {
            if (obj == null || pool.Contains(obj)) return; // ponytail: Contains is O(n); fine for small pools, use a HashSet if pools grow large
            obj.gameObject.Hide();
            pool.Enqueue(obj);
        }
    }
}

/*
-------------------------
CÁCH SỬ DỤNG ObjectPool<T>
-------------------------

// 1. Tạo prefab (ví dụ: Bullet) và kéo vào Inspector hoặc load qua Resources

// 2. Khởi tạo pool:
public ObjectPool<Bullet> bulletPool;

void Start()
{
    bulletPool = new ObjectPool<Bullet>(bulletPrefab, 20); // 20 là số lượng khởi tạo
}

// 3. Lấy object từ pool khi cần:
Bullet bullet = bulletPool.Get();
bullet.transform.position = firePosition;
bullet.Init(...); // nếu có

// 4. Khi không dùng nữa (ví dụ: bullet trúng địch hoặc ra khỏi màn hình), trả về pool:
bulletPool.Release(bullet);

*/
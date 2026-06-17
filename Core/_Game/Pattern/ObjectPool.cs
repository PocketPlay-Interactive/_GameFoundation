using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Queue<T> pool = new();

    public ObjectPool(T prefab, int initialSize = 10)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
            pool.Enqueue(Object.Instantiate(prefab));
    }

    public T Get()
    {
        if (pool.Count == 0)
            pool.Enqueue(Object.Instantiate(prefab));
        var obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
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
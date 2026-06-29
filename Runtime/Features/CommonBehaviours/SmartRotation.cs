using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartRotation : MonoBehaviour
{
    public float speed = 90.0f; // Độ nhanh của việc xoay
    public bool IsActive = false;
    public Vector3 directionRotation = new Vector3(0.0f, 0.0f, -1.0f);

    [Header("Rotation Options")]
    [Tooltip("If true, rotation will be independent of Time.timeScale.")]
    public bool useUnscaledTime = false; // Thêm option này

    public void Init()
    {
        IsActive = true;
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (!IsActive)
            return;

        // Chọn deltaTime phù hợp dựa trên cờ useUnscaledTime
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        transform.Rotate(directionRotation * speed * deltaTime);
    }
}

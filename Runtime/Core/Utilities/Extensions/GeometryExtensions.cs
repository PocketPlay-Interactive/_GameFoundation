using System;
using UnityEngine;

public static class GeometryExtensions
{
    public static Quaternion ToRotation(this Vector3 eulerAngles) => Quaternion.Euler(eulerAngles);
    public static Vector3 ToEulerAngles(this Quaternion quaternion) => quaternion.eulerAngles;

    /// <summary>
    /// Move along a circle by arc length "distance".
    /// </summary>
    public static Vector2 TravelAlongCircle(this Vector2 pos, Vector2 center, float distance)
    {
        Vector3 axis = Vector3.back;
        Vector2 dir = pos - center;
        float circumference = 2f * Mathf.PI * dir.magnitude;
        float angle = distance / Mathf.Max(circumference, 1e-6f) * 360f;
        dir = Quaternion.AngleAxis(angle, axis) * dir;
        return dir + center;
    }

    /// <summary>
    /// Position on circumference in XZ plane at "degrees".
    /// </summary>
    public static Vector3 GetPointOnCircleXZ(Vector3 center, float radius, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float x = Mathf.Cos(radians);
        float z = Mathf.Sin(radians);
        Vector3 position = new Vector3(x, center.y, z) * radius + center;
        return position;
    }

    /// <summary>
    /// Random point on a discrete circle ring (integer X/Y). Uses Random.Range properly.
    /// </summary>
    public static Vector3 GetRandomCirclePosition(double radius, Vector3 center)
    {
        int point = UnityEngine.Random.Range(5, 10);
        double slice = 2 * Math.PI / point;
        double angle = slice * UnityEngine.Random.Range(0, point);
        int newX = (int)(center.x + radius * Math.Cos(angle));
        int newY = (int)(center.y + radius * Math.Sin(angle));
        return new Vector3(newX, newY);
    }

    /// <summary>
    /// Random unit direction in XZ plane.
    /// </summary>
    public static Vector3 GetRandomDirectionXZ()
    {
        return Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
    }
}

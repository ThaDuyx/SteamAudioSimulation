using UnityEngine;
struct Dimensions
{
    public static Vector3 lowerThreshold = new(-6.0f, 1.6f, -3f);
    public static Vector3 upperThreshold = new(6f, 2f, 12f);
    public static Vector3 defaultLocation = new();

    public static Vector3 lowerSourceThreshold = new(-6.0f, 1.6f, -3f);
    public static Vector3 upperSourceThreshold = new(6f, 2f, 12f);
    public static Vector3 defaultSourceLocation = new();
}
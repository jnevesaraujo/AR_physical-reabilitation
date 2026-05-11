using Mediapipe.Tasks.Components.Containers;
using Unity.VisualScripting;
using UnityEngine;

public static class WorldLandmarkProvider
{
    public static Landmarks CurrentLandmarks { get; private set; }

    public static void UpdateLandmarks(Landmarks landmarks)
    {
        CurrentLandmarks = landmarks;
    }

    public static bool IsReady =>
        CurrentLandmarks.landmarks != null &&
        CurrentLandmarks.landmarks.Count > 0;

    public static Vector3 GetPosition(int index)
    {
        if (!IsReady || index < 0 || index >= CurrentLandmarks.landmarks.Count)
            return Vector3.zero;

        var lm = CurrentLandmarks.landmarks[index];
        return new Vector3(lm.x, -lm.y, lm.z); // negate Y for Unity coordinate system
    }
}
using UnityEngine;

namespace App.Vision.Guides
{
    public interface IExerciseGuide
    {
        // Called once on first calibration tap: places the rest-position marker
        void Initialize(Vector3 anchorPos, float bodyScale);

        // Called on second calibration tap (exercises that have a two-step flow)
        // No-op for exercises that don't need it (NeckRotation, HandGrip)
        void PlacePeakMarker(Vector3 peakPos, float bodyScale);

        // Called every frame after calibration
        void UpdateVisuals(Vector3 trackedPos, float progress);

        // Called when a rep completes
        void PlaySuccess();

        // Called when posture is good/bad
        void SetPostureFeedback(bool isGood);

        // Cleanup called before guide is destroyed
        void Cleanup();
    }
}
using UnityEngine;

namespace App.Vision.Guides
{
    public class HandGripGuide : MonoBehaviour, IExerciseGuide
    {
        [Tooltip("The energy sphere that squashes on grip (assign in prefab)")]
        public Transform energySphere;
        public ParticleSystem successParticles;
        private float zOffset = 5f; // Z offset to keep guide in front of body
        private Renderer _sphereRenderer;
        private Vector3 _originalScale;

        public void Initialize(Vector3 palmCenter, float bodyScale)
        {
            transform.position = new Vector3(palmCenter.x, palmCenter.y, palmCenter.z - zOffset);
            if (energySphere != null)
            {
                _sphereRenderer = energySphere.GetComponent<Renderer>();
                _originalScale = energySphere.localScale;

                // Scale relative to hand size
                float s = Mathf.Max(bodyScale * 1.5f, 5f);
                energySphere.localScale = Vector3.one * s;
                _originalScale = energySphere.localScale;
            }

            // Disable any LineRenderer that may be on the prefab root
            var lr = GetComponent<LineRenderer>();
            if (lr != null) lr.enabled = false;

            transform.position = palmCenter;

            if (successParticles == null)
                successParticles = GetComponentInChildren<ParticleSystem>();
        }

        // HandGrip has no peak step
        public void PlacePeakMarker(Vector3 peakPos, float bodyScale) { }

        // progress here is the holdProgress (0→1 isometric timer)
        // apertureRatio must be passed; we repurpose the Vector3 to carry it via x
        // — see UpdateHandGripVisuals helper below for the clean call
        public void UpdateVisuals(Vector3 trackedPos, float progress)
        {
            // Standard path — trackedPos is palm center, progress is hold progress
            // apertureRatio unavailable here so sphere just tracks position
            transform.position = new Vector3(trackedPos.x, trackedPos.y, trackedPos.z - zOffset);
        }

        // Convenience overload used by HandGripExtractor so the caller
        // doesn't need to pack values into a Vector3
        public void UpdateHandGripVisuals(Vector3 palmCenter, float apertureRatio, float holdProgress)
        {
            // Move the guide root to follow the palm — Z offset keeps it in front of body
            transform.position = new Vector3(
                palmCenter.x,
                palmCenter.y,
                palmCenter.z - zOffset);

            if (energySphere == null) return;

            // Scale the sphere based on aperture
            float scaleFactor = Mathf.Clamp(apertureRatio, 0.2f, 1.2f);
            energySphere.localScale = _originalScale * scaleFactor;

            // Colour shifts from blue (open) to orange (held)
            if (_sphereRenderer != null)
                _sphereRenderer.material.color =
                    Color.Lerp(Color.blue, new Color(1f, 0.5f, 0f), holdProgress);
        }

        public void PlaySuccess()
        {
            if (successParticles == null) return;
            var main = successParticles.main;
            main.startColor = Color.yellow;
            var emission = successParticles.emission;
            emission.rateOverTime = 0;
            successParticles.Emit(30);
        }

        public void SetPostureFeedback(bool isGood) { }

        public void Cleanup() { }
    }
}
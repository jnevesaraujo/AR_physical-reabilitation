using UnityEngine;

namespace App.Vision.Guides
{
    public class HandGripGuide : MonoBehaviour, IExerciseGuide
    {
        [Tooltip("The energy sphere that squashes on grip (assign in prefab)")]
        public Transform energySphere;
        public ParticleSystem successParticles;

        private Renderer _sphereRenderer;
        private Vector3 _originalScale;

        public void Initialize(Vector3 palmCenter, float bodyScale)
        {
            if (energySphere != null)
            {
                _sphereRenderer = energySphere.GetComponent<Renderer>();
                _originalScale  = energySphere.localScale;

                // Scale relative to hand size
                float s = bodyScale * 2f;
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
            // trackedPos.x carries apertureRatio, progress carries holdProgress
            float apertureRatio = trackedPos.x;
            float holdProgress  = progress;

            // Move the guide to follow the palm
            transform.position = new Vector3(trackedPos.y, trackedPos.z, transform.position.z - 2f);

            if (energySphere == null) return;

            float scaleFactor = Mathf.Clamp(apertureRatio, 0.2f, 1.2f);
            energySphere.localScale = _originalScale * scaleFactor;

            if (_sphereRenderer != null)
                _sphereRenderer.material.color =
                    Color.Lerp(Color.blue, new Color(1f, 0.5f, 0f), holdProgress);
        }

        // Convenience overload used by HandGripExtractor so the caller
        // doesn't need to pack values into a Vector3
        public void UpdateHandGripVisuals(Vector3 palmCenter, float apertureRatio, float holdProgress)
        {
            // Pack into the UpdateVisuals convention:
            // x = apertureRatio, y/z = palm XY, transform.z handled internally
            UpdateVisuals(new Vector3(apertureRatio, palmCenter.x, palmCenter.y), holdProgress);
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
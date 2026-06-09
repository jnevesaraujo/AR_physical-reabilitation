using UnityEngine;

namespace App.Vision.Guides
{
    public class ShoulderGuide : MonoBehaviour, IExerciseGuide
    {
        [Tooltip("Sphere that slides along the track (assign in prefab)")]
        public Transform sliderSphere;
        public ParticleSystem successParticles;

        private float zOffset = 3f;
        private LineRenderer _line;
        private Renderer _sphereRenderer;
        private bool _ready = false;
        private float _startY;

        public void Initialize(Vector3 restPos, float bodyScale)
        {
            _startY = restPos.y;

            _line = GetComponent<LineRenderer>();
            if (_line == null) _line = gameObject.AddComponent<LineRenderer>();

            // Rail drawn after peak is confirmed in PlacePeakMarker;
            // for now just position the sphere at rest
            if (sliderSphere != null)
            {
                _sphereRenderer = sliderSphere.GetComponent<Renderer>();
                float sphereScale = bodyScale * 0.3f;
                sliderSphere.localScale = Vector3.one * sphereScale;
                sliderSphere.position = new Vector3(restPos.x, restPos.y, restPos.z - zOffset);
            }

            if (successParticles == null)
                successParticles = GetComponentInChildren<ParticleSystem>();
        }

        // Called when patient confirms peak position
        public void PlacePeakMarker(Vector3 peakPos, float bodyScale)
        {
            if (_line == null) return;

            float amplitude = Mathf.Abs(peakPos.y - _startY);
            float margin = amplitude * 0.15f;
            float lineWidth = bodyScale * 0.03f;

            _line.enabled = true;
            _line.useWorldSpace = true;
            _line.positionCount = 2;

            // Start position uses stored _startY so the rail aligns with calibration
            var startPos = new Vector3(peakPos.x, _startY - margin, peakPos.z - zOffset);
            var endPos = new Vector3(peakPos.x, peakPos.y + margin, peakPos.z - zOffset);

            _line.SetPosition(0, startPos);
            _line.SetPosition(1, endPos);
            _line.startWidth = lineWidth;
            _line.endWidth = lineWidth;
            _line.startColor = new Color(0.94f, 0.8f, 1f, 0.5f);
            _line.endColor = new Color(0.94f, 0.8f, 1f, 0.5f);

            _ready = true;
        }

        public void UpdateVisuals(Vector3 wristPos, float progress)
        {
            if (sliderSphere == null) return;

            sliderSphere.position = new Vector3(wristPos.x, wristPos.y, wristPos.z - zOffset);

            if (_sphereRenderer == null) return;

            if (!_ready)
            {
                // Still in discovery phase — blue sphere
                _sphereRenderer.material.color = Color.blue;
            }
            else
            {
                // Active phase — yellow → green
                _sphereRenderer.material.color =
                    Color.Lerp(Color.yellow, Color.green, progress);
            }
        }

        public void PlaySuccess()
        {
            if (successParticles == null) return;
            var main = successParticles.main;
            main.startColor = new Color(0.11f, 0.62f, 0.46f);
            var emission = successParticles.emission;
            emission.rateOverTime = 0;
            successParticles.Emit(30);
        }

        public void SetPostureFeedback(bool isGood)
        {
            if (_line == null) return;
            _line.startColor = isGood ? new Color(1f, 1f, 1f, 0.5f) : Color.red;
            _line.endColor = isGood ? new Color(1f, 1f, 1f, 0.5f) : Color.red;
        }

        public void Cleanup() { }
    }
}
using UnityEngine;

namespace App.Vision.Guides
{
    public class NeckGuide : MonoBehaviour, IExerciseGuide
    {
        [Tooltip("The pacer object that orbits the circle (assign in prefab)")]
        public Transform pacerObject;
        [Tooltip("Particle system on the pacer")]
        public ParticleSystem successParticles;

        private LineRenderer _line;
        private float _radius;
        private float _targetSeconds;
        private float _currentAngle = 0f;
        private bool _isRunning = false;
        private bool _wasSynchronized = false;
        private Vector3 _worldCenter;

        // NeckGuide uses a two-value Initialize — radius passed as bodyScale,
        // targetSeconds stored separately via a dedicated setter called by NeckRotationExtractor
        private float _cachedTargetSeconds;
        public void SetPacerSpeed(float targetSecondsPerRep)
        {
            _cachedTargetSeconds = targetSecondsPerRep;
        }

        public void Initialize(Vector3 anchorPos, float bodyScale)
        {

            Vector3 safeAnchor = new Vector3(anchorPos.x, anchorPos.y, 70f);
            transform.position = safeAnchor;

            _radius = bodyScale;   // caller passes shoulderWidth * 0.4f
            _targetSeconds = _cachedTargetSeconds > 0f ? _cachedTargetSeconds : 4f;
            _worldCenter = safeAnchor;

            _line = GetComponent<LineRenderer>();
            if (_line == null) _line = gameObject.AddComponent<LineRenderer>();

            DrawCircle();

            if (pacerObject != null && successParticles == null)
                successParticles = pacerObject.GetComponentInChildren<ParticleSystem>();

            _currentAngle = 0f;
            _isRunning = true;
        }

        // NeckRotation has no peak step
        public void PlacePeakMarker(Vector3 peakPos, float bodyScale) { }

        public void UpdateVisuals(Vector3 nosePos, float progress)
        {
            _worldCenter = nosePos;
            transform.position = nosePos;
            EvaluateSynchronization(nosePos);
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

        public void SetPostureFeedback(bool isGood)
        {
            if (_line == null) return;
            _line.startColor = isGood ? Color.white : Color.red;
            _line.endColor = isGood ? Color.white : Color.red;
        }

        public void Cleanup() { }

        private void Update()
        {
            if (!_isRunning || _targetSeconds <= 0f || pacerObject == null) return;

            _currentAngle += 360f / _targetSeconds * Time.deltaTime;
            if (_currentAngle >= 360f) _currentAngle -= 360f;

            float rad = _currentAngle * Mathf.Deg2Rad;
            pacerObject.localPosition = new Vector3(
                Mathf.Cos(rad) * _radius,
                Mathf.Sin(rad) * _radius,
                -0.05f);
        }

        private void DrawCircle()
        {
            _line.useWorldSpace = false;
            _line.loop = true;
            _line.positionCount = 50;

            float w = _radius * 0.05f;
            _line.startWidth = w;
            _line.endWidth = w;

            float angle = 0f;
            for (int i = 0; i < 50; i++)
            {
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * _radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * _radius;
                _line.SetPosition(i, new Vector3(x, y, 0f));
                angle += 360f / 50f;
            }
        }

        private void EvaluateSynchronization(Vector3 nosePos)
        {
            if (pacerObject == null || successParticles == null) return;

            float pacerAngle = _currentAngle;

            Vector2 offset = new Vector2(
                nosePos.x - _worldCenter.x,
                nosePos.y - _worldCenter.y);
            float noseAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            if (noseAngle < 0f) noseAngle += 360f;

            float diff = Mathf.Abs(Mathf.DeltaAngle(pacerAngle, noseAngle));
            bool isSynchronized = diff <= 45f;

            if (isSynchronized == _wasSynchronized) return;
            _wasSynchronized = isSynchronized;

            var main = successParticles.main;
            var emission = successParticles.emission;

            if (isSynchronized)
            {
                main.startColor = Color.green;
                emission.rateOverTime = 60f;
            }
            else
            {
                main.startColor = new Color(1f, 0.5f, 0f);
                emission.rateOverTime = 15f;
            }

            successParticles.Clear();
            successParticles.Play();
        }
    }
}
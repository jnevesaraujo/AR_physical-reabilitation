using UnityEngine;
using App.Vision.Guides;

namespace App.Vision.Guides
{
    public class ElbowGuide : MonoBehaviour, IExerciseGuide
    {
        // Assigned in prefab Inspector
        public Transform wristSphere;
        public ParticleSystem successParticles;

        private GameObject _restRing;
        private GameObject _peakRing;
        private Renderer _wristRenderer;
        private Renderer _peakRingRenderer;
        private bool _ready = false;

        public void Initialize(Vector3 restPos, float bodyScale)
        {
/*             var canvas = FindFirstObjectByType<Canvas>();
            Debug.Log($"Canvas planeDistance={canvas.planeDistance}"); */
            float ringRadius = bodyScale * 0.3f;
            float sphereScale = bodyScale * 0.3f;

            _restRing = CreateRing("RestRing", new Color(0.6f, 0.6f, 0.6f, 0.5f), ringRadius);           
            _restRing.transform.position = new Vector3(restPos.x, restPos.y,  restPos.z - 5f);

            if (wristSphere != null)
            {
                _wristRenderer = wristSphere.GetComponent<Renderer>();
                wristSphere.localScale = Vector3.one * sphereScale;
                if (_wristRenderer != null)
                    _wristRenderer.material.color = Color.white;
                wristSphere.position = restPos;
            }

            if (successParticles == null)
                successParticles = GetComponentInChildren<ParticleSystem>();
        }

        public void PlacePeakMarker(Vector3 peakPos, float bodyScale)
        {
            float ringRadius = bodyScale * 0.4f;

            _peakRing = CreateRing("PeakRing", new Color(0.11f, 0.62f, 0.46f, 0.85f), ringRadius);
            _peakRing.transform.position = new Vector3(peakPos.x, peakPos.y,  peakPos.z - 5f);
            _peakRingRenderer = _peakRing.GetComponentInChildren<Renderer>();

            _ready = true;
        }

        public void UpdateVisuals(Vector3 wristPos, float progress)
        {
            if (!_ready || wristSphere == null) return;

            wristSphere.position = wristPos;

            if (_wristRenderer != null)
                _wristRenderer.material.color = Color.Lerp(
                    Color.white, new Color(0.11f, 0.62f, 0.46f), progress);

            if (_peakRingRenderer != null)
            {
                float proximity = Mathf.Clamp01(progress * 1.2f);
                _peakRingRenderer.material.color = Color.Lerp(
                    new Color(0.11f, 0.62f, 0.46f, 0.4f),
                    new Color(0.11f, 0.62f, 0.46f, 1.0f),
                    proximity);
            }
        }

        public void PlaySuccess()
        {
            if (successParticles == null) return;
            successParticles.transform.position = _peakRing != null
                ? _peakRing.transform.position
                : transform.position;

            var main = successParticles.main;
            main.startColor = new Color(0.11f, 0.62f, 0.46f);
            var emission = successParticles.emission;
            emission.rateOverTime = 0;
            successParticles.Emit(30);
        }

        public void SetPostureFeedback(bool isGood)
        {
            if (_wristRenderer != null && !isGood)
                _wristRenderer.material.color = Color.red;
        }

        public void Cleanup()
        {
            if (_restRing != null) Destroy(_restRing);
            if (_peakRing != null) Destroy(_peakRing);
        }

        private GameObject CreateRing(string ringName, Color color, float radius)
        {
            var go = new GameObject(ringName);
            go.transform.SetParent(transform);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 32;

            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            lr.material = mat;

            float thickness = radius * 0.18f;
            lr.startWidth = thickness;
            lr.endWidth = thickness;

            for (int i = 0; i < 32; i++)
            {
                float angle = (i / 32f) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius, 0f));
            }
            return go;
        }
    }
}
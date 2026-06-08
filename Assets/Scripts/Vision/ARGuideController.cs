using UnityEngine;

namespace App.Vision
{
    //    [RequireComponent(typeof(LineRenderer))]
    public class ARGuideController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Drag the child 'Glitter' Particle System here.")]
        public Transform pacerObject;

        [Header("Hand Grip Settings")]
        [Tooltip("The object that will scale up and down like an energy sphere.")]
        public Transform energySphere;
        [Header("Shoulder Slide Settings")]
        [Tooltip("Sphere which will slide along the line.")]
        public Transform shoulderSliderSphere;
        [Header("Elbow Guide Settings")]
        [Tooltip("Small dot placed at the elbow joint to show the rotation pivot.")]
        public Transform elbowPivotDot;
        [Tooltip("Particle system that fires on rep completion. Direct reference avoids child-search ambiguity.")]
        public ParticleSystem successParticles;

        private LineRenderer _line;
        private ParticleSystem _particles;
        // VVariables Neck
        private float _targetSeconds;
        private float _radius;
        private float _currentAngle = 0f;
        private bool _isRunning = false;
        private bool _wasSynchronized = false;
        private Vector3 _worldCenter;

        // Variables Hand
        private bool _isHandMode = false;
        private Vector3 _originalSphereScale;

        // Variables Shoulder
        private bool _isShoulderMode = false;
        private Renderer _shoulderRenderer;

        // Variables Elbow
        private bool _isElbowMode = false;
        private Vector3 _elbowPivot;
        private float _arcRadius;
        private float _restAngleDeg;
        private float _peakAngleDeg;
        private Renderer _elbowSphereRenderer;
        private bool _isElbowRingMode = false;
        private GameObject _restRing;
        private GameObject _peakRing;
        private Transform _wristIndicator;
        private Renderer _wristRenderer;
        private Renderer _peakRingRenderer;


        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            if (successParticles != null)
                _particles = successParticles;
        }

        public void InitializeGuide(float radius, float targetSeconds, Vector3 worldCenter)
        {
            _radius = radius;
            _targetSeconds = targetSeconds;
            _worldCenter = worldCenter;

            _particles = pacerObject.GetComponentInChildren<ParticleSystem>();

            DrawStaticCircle();

            _currentAngle = 0f;
            _isRunning = true;
        }

        private void DrawStaticCircle()
        {
            _line.useWorldSpace = false;
            _line.loop = true;
            _line.positionCount = 50;

            // Set thickness based on radius
            _line.startWidth = _radius * 0.05f;
            _line.endWidth = _radius * 0.05f;

            float angle = 0f;
            for (int i = 0; i < 50; i++)
            {
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * _radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * _radius;
                _line.SetPosition(i, new Vector3(x, y, 0f));
                angle += 360f / 50f;
            }
        }

        void Update()
        {
            if (_isHandMode || !_isRunning || _targetSeconds <= 0f || pacerObject == null) return;

            // Move the angle
            _currentAngle += (360f / _targetSeconds) * Time.deltaTime;
            if (_currentAngle >= 360f) _currentAngle -= 360f;

            // Apply to pacer position (slightly in front of the line)
            float angleInRadians = _currentAngle * Mathf.Deg2Rad;
            pacerObject.localPosition = new Vector3(
                Mathf.Cos(angleInRadians) * _radius,
                Mathf.Sin(angleInRadians) * _radius,
                -0.05f
            );
        }
        public void EvaluateSynchronization(Vector3 userNosePosition)
        {
            if (pacerObject == null || _particles == null) return;

            // Angle of the pacer (already known)
            float pacerAngle = _currentAngle;

            // Angle of the nose relative to the guide center
            Vector2 noseOffset = new Vector2(
                userNosePosition.x - _worldCenter.x,
                userNosePosition.y - _worldCenter.y
            );
            float noseAngle = Mathf.Atan2(noseOffset.y, noseOffset.x) * Mathf.Rad2Deg;
            if (noseAngle < 0) noseAngle += 360f;

            // Compare angles with a degree tolerance instead of distance
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(pacerAngle, noseAngle));
            float angleTolerance = 45f; // degrees — adjust to taste

            bool isSynchronized = angleDiff <= angleTolerance;

            if (isSynchronized == _wasSynchronized) return;
            _wasSynchronized = isSynchronized;

            var main = _particles.main;
            var emission = _particles.emission;

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

            _particles.Clear();
            _particles.Play();
        }

        public void InitializeHandGuide(Vector3 centerPos)
        {
            _isHandMode = true;
            if (energySphere != null)
            {
                energySphere.localPosition = Vector3.zero;
                _originalSphereScale = energySphere.localScale;
            }
            if (_line != null) _line.enabled = false; // Desliga o círculo do pescoço se existir no prefab
        }

        public void UpdateEnergySphere(Vector3 centerPosition, float apertureRatio, float holdProgress)
        {
            if (!_isHandMode) return;

            // 1. Atualiza a posição do prefab para estar sempre no meio dos dedos
            transform.position = new Vector3(centerPosition.x, centerPosition.y, centerPosition.z - 2f);

            if (energySphere != null)
            {
                // 2. Esmaga a esfera. Se o rácio diminui, a escala diminui.
                // Usamos Mathf.Clamp para garantir que a esfera não fica invisível nem gigante.
                float scaleFactor = Mathf.Clamp(apertureRatio, 0.2f, 1.2f);
                energySphere.localScale = _originalSphereScale * scaleFactor;

                // 3. Muda a cor consoante o tempo isométrico.
                // Quando o holdProgress chega a 1.0, o exercício termina.
                Renderer sphereRenderer = energySphere.GetComponent<Renderer>();
                if (sphereRenderer != null)
                {
                    // Interpola do Azul Escuro (0%) para Laranja Brilhante (100%)
                    sphereRenderer.material.color = Color.Lerp(Color.blue, new Color(1f, 0.5f, 0f), holdProgress);
                }
            }
        }

        public void InitializeShoulderGuide(Vector3 startPos, float maxY)
        {
            _isShoulderMode = true;

            // Calcula a distância para sabermos onde a linha acaba
            float amplitude = Mathf.Abs(maxY - startPos.y);
            float margin = amplitude * 0.15f;
            float lineWidth = 1.5f;
            // 1. Configura a Linha (O Carril)
            if (_line != null)
            {
                _line.enabled = true;
                _line.useWorldSpace = true;
                _line.positionCount = 2;

                // O comprimento adapta-se ao paciente
                _line.SetPosition(0, new Vector3(startPos.x, startPos.y - margin, startPos.z));
                _line.SetPosition(1, new Vector3(startPos.x, maxY + margin, startPos.z));

                _line.startWidth = lineWidth;
                _line.endWidth = lineWidth;

                _line.startColor = new Color(1f, 1f, 1f, 0.5f);
                _line.endColor = new Color(1f, 1f, 1f, 0.5f);
            }

            // 2. Configura a Esfera
            if (shoulderSliderSphere != null)
            {
                _shoulderRenderer = shoulderSliderSphere.GetComponent<Renderer>();

                shoulderSliderSphere.position = new Vector3(startPos.x, startPos.y, startPos.z);
                float perfectSphereScale = lineWidth * 3f;
                shoulderSliderSphere.localScale = new Vector3(perfectSphereScale, perfectSphereScale, perfectSphereScale);

                if (_particles == null)
                {
                    _particles = shoulderSliderSphere.GetComponentInChildren<ParticleSystem>();
                }
            }
        }

        public void UpdateShoulderGuide(Vector3 wristPos, float progress, bool isDiscovering, float zOffset)
        {
            if (!_isShoulderMode) return;

            // 1. A Esfera segue exatamente a mão do paciente (incluindo desvios)
            if (shoulderSliderSphere != null)
            {
                shoulderSliderSphere.position = new Vector3(wristPos.x, wristPos.y, wristPos.z + zOffset);
            }

            // 2. Feedback de Cor na Esfera
            if (_shoulderRenderer != null)
            {
                if (isDiscovering)
                {
                    _shoulderRenderer.material.color = Color.blue;
                }
                else
                {
                    _shoulderRenderer.material.color = Color.Lerp(Color.yellow, Color.green, progress);
                }
            }
        }

        /*         public void PlaySuccessParticles()
                {
                    if (_particles != null)
                    {
                        // Reinicia o rasto visual da estrela para modo explosão
                        var main = _particles.main;
                        main.startColor = Color.yellow;

                        var emission = _particles.emission;
                        emission.rateOverTime = 0; // Para a emissão contínua

                        // Dispara um Burst (explosão de 30 partículas)
                        _particles.Emit(30);
                    }
                } */

        public void PlaySuccessParticles()
        {
            if (_particles != null)
            {
                _particles.transform.position = _peakRing != null
                    ? _peakRing.transform.position
                    : transform.position;

                var main = _particles.main;
                main.startColor = new Color(0.11f, 0.62f, 0.46f);
                var emission = _particles.emission;
                emission.rateOverTime = 0;
                _particles.Emit(30);
            }
        }

        public void SetColor(Color targetColor)
        {
            if (_line != null)
            {
                _line.startColor = targetColor;
                _line.endColor = targetColor;
            }
        }
        /*
                public void InitializeElbowGuide(
             Vector3 elbowPivot,
             Vector3 wristAtRest,
             Vector3 wristAtPeak,
             float targetSecondsPerRep,
             float zOffset = -0.05f)
                {
                    _isElbowMode = true;
                    _elbowPivot = elbowPivot;
                    _arcRadius = Vector3.Distance(elbowPivot, wristAtRest);
                    _targetSeconds = targetSecondsPerRep;

                    if (elbowPivotDot != null)
                        elbowPivotDot.position = new Vector3(elbowPivot.x, elbowPivot.y, elbowPivot.z + zOffset);

                    // Derive the two angle bounds from the actual wrist positions.
                    // We work in the elbow's local XY plane (ignoring Z depth).
                    Vector2 toRest = new Vector2(
                        wristAtRest.x - elbowPivot.x,
                        wristAtRest.y - elbowPivot.y);
                    Vector2 toPeak = new Vector2(
                        wristAtPeak.x - elbowPivot.x,
                        wristAtPeak.y - elbowPivot.y);

                    _restAngleDeg = Mathf.Atan2(toRest.y, toRest.x) * Mathf.Rad2Deg;
                    _peakAngleDeg = Mathf.Atan2(toPeak.y, toPeak.x) * Mathf.Rad2Deg;

                    if (_line != null)
                    {
                        _line.enabled = true;
                        _line.useWorldSpace = true;
                        _line.loop = false;

                        DrawArc();

                        float w = _arcRadius * 0.08f;
                        _line.startWidth = w;
                        _line.endWidth = w;
                        _line.startColor = new Color(1f, 1f, 1f, 0.4f);
                        _line.endColor = new Color(1f, 1f, 1f, 0.4f);
                    }

                    // The pacer sphere slides along the arc
                    if (shoulderSliderSphere != null)
                    {
                        _elbowSphereRenderer = shoulderSliderSphere.GetComponent<Renderer>();
                        float sphereScale = _arcRadius * 0.18f;
                        shoulderSliderSphere.localScale = Vector3.one * sphereScale;

                        // Place it at rest position to start
                        shoulderSliderSphere.position = ArcPoint(_restAngleDeg);
                    }

                    if (_particles == null && shoulderSliderSphere != null)
                        _particles = shoulderSliderSphere.GetComponentInChildren<ParticleSystem>();

                    _currentAngle = _restAngleDeg;
                    _isRunning = true;
                }

                private void DrawArc()
                {
                    int segments = 40;
                    _line.positionCount = segments + 1;

                    float startA = _restAngleDeg;
                    float endA = _peakAngleDeg;

                    // Ensure we always sweep the short way (flexion arc, not reflex)
                    float delta = Mathf.DeltaAngle(startA, endA);

                    for (int i = 0; i <= segments; i++)
                    {
                        float t = i / (float)segments;
                        float angle = startA + delta * t;
                        _line.SetPosition(i, ArcPoint(angle));
                    }
                }

                private Vector3 ArcPoint(float angleDeg, float zOffset = -0.05f)
                {
                    float rad = angleDeg * Mathf.Deg2Rad;
                    return new Vector3(
                        _elbowPivot.x + Mathf.Cos(rad) * _arcRadius,
                        _elbowPivot.y + Mathf.Sin(rad) * _arcRadius,
                        _elbowPivot.z + zOffset
                    );
                }

                public void UpdateElbowGuide(Vector3 wristPos, float progress, float zOffset = -0.05f)
                {
                    if (!_isElbowMode) return;

                    if (shoulderSliderSphere != null)
                    {
                        shoulderSliderSphere.position = new Vector3(
                            wristPos.x,
                            wristPos.y,
                            _elbowPivot.z + zOffset);
                    }

                    // 2. Colour: yellow at rest → green at peak
                    if (_elbowSphereRenderer != null)
                        _elbowSphereRenderer.material.color = Color.Lerp(Color.yellow, Color.green, progress);

                    // 3. Recolour the arc to show how much of the ROM has been completed
                    if (_line != null)
                    {
                        Color arcCol = Color.Lerp(
                            new Color(1f, 1f, 1f, 0.3f),
                            new Color(0.3f, 1f, 0.4f, 0.7f),
                            progress);
                        _line.startColor = arcCol;
                        _line.endColor = arcCol;
                    }
                }
            } */

        // Call this on first calibration (arm at rest, button tapped)
        public void PlaceRestRing(Vector3 wristWorldPos, float armLengthEstimate)
        {
            _isElbowRingMode = true;

            float ringRadius = armLengthEstimate * 0.4f;   // ~12–24px — wrist-sized ring
            float sphereScale = armLengthEstimate * 0.3f;   // ~9–18px — visible sphere

            if (_restRing == null)
                _restRing = CreateRing("RestRing",
                                       new Color(0.6f, 0.6f, 0.6f, 0.5f),
                                       ringRadius);

            _restRing.transform.position = wristWorldPos;

            if (shoulderSliderSphere != null)
            {
                _wristIndicator = shoulderSliderSphere;
                _wristRenderer = _wristIndicator.GetComponent<Renderer>();
                _wristIndicator.localScale = Vector3.one * sphereScale;
                if (_wristRenderer != null)
                    _wristRenderer.material.color = Color.white;
                _wristIndicator.position = wristWorldPos;
            }

            if (_line != null) _line.enabled = false;

            // Find particles
            if (_particles == null)
                _particles = GetComponentInChildren<ParticleSystem>();
        }

        // Call this when the patient confirms peak position
        public void PlacePeakRing(Vector3 wristWorldPos, float armLengthEstimate, float zOffset = -0.05f)
        {
            float ringRadius = armLengthEstimate * 0.4f;   // same size as rest ring for consistency

            if (_peakRing == null)
                _peakRing = CreateRing("PeakRing", new Color(0.11f, 0.62f, 0.46f, 0.85f), ringRadius);

            _peakRing.transform.position = new Vector3(
                wristWorldPos.x, wristWorldPos.y, wristWorldPos.z + zOffset);

            _peakRingRenderer = _peakRing.GetComponentInChildren<Renderer>();
        }

        // Call every frame after both rings are placed
        public void UpdateElbowRings(Vector3 wristWorldPos, float progress, float zOffset = -0.05f)
        {
            if (!_isElbowRingMode || _wristIndicator == null) return;

            _wristIndicator.position = new Vector3(
                wristWorldPos.x, wristWorldPos.y, wristWorldPos.z + zOffset);

            // Colour: white at rest → green at peak
            if (_wristRenderer != null)
                _wristRenderer.material.color = Color.Lerp(Color.white,
                                                            new Color(0.11f, 0.62f, 0.46f),
                                                            progress);

            // Pulse the peak ring brighter when the wrist is close
            if (_peakRingRenderer != null)
            {
                float proximity = Mathf.Clamp01(progress * 1.2f); // slightly anticipates
                _peakRingRenderer.material.color = Color.Lerp(
                    new Color(0.11f, 0.62f, 0.46f, 0.4f),
                    new Color(0.11f, 0.62f, 0.46f, 1.0f),
                    proximity);
            }
        }



        // ── Ring factory ──────────────────────────────────────────────
        private GameObject CreateRing(string name, Color color, float radius)
        {
            var go = new GameObject(name);
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
                    Mathf.Sin(angle) * radius,
                    0f));
            }
            return go;
        }



    }
}
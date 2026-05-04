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
        private LineRenderer _line;
        private ParticleSystem _particles;
        // Váriaveis Neck
        private float _targetSeconds;
        private float _radius;
        private float _currentAngle = 0f;
        private bool _isRunning = false;
        private bool _wasSynchronized = false;
        private Vector3 _worldCenter;

        // Variáveis Hand
        private bool _isHandMode = false;
        private Vector3 _originalSphereScale;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
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
                angle += (360f / 50f);
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

        public void PlaySuccessParticles()
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
        }

        public void SetColor(Color targetColor)
        {
            if (_line != null)
            {
                _line.startColor = targetColor;
                _line.endColor = targetColor;
            }
        }
    }
}
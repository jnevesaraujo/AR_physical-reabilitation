using UnityEngine;

namespace App.Vision
{
    [RequireComponent(typeof(LineRenderer))]
    public class ARGuideController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Drag the child 'Glitter' Particle System here.")]
        public Transform pacerObject; 

        private LineRenderer _line;
        private float _targetSeconds;
        private float _radius;
        private float _currentAngle = 0f;
        private bool _isRunning = false;

        public void InitializeGuide(float radius, float targetSeconds)
        {
            _radius = radius;
            _targetSeconds = targetSeconds;
            
            DrawStaticCircle();
            
            _currentAngle = 0f;
            _isRunning = true;
        }

        private void DrawStaticCircle()
        {
            _line = GetComponent<LineRenderer>();
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
            if (!_isRunning || _targetSeconds <= 0f || pacerObject == null) return;

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
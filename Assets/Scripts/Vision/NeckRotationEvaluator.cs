using UnityEngine;
using App.Data.ScriptableObjects;
using System;
using System.Collections.Generic;

namespace App.Vision
{
    public class NeckRotationEvaluator
    {
        private readonly NeckRotationDefinition _definition;
        private Vector2 _centerOrigin;
        private float _initialShoulderDiff;
        private bool _isCalibrated = false;
        private bool[] _reachedQuadrants = new bool[4];
        private List<int> _quadrantSequence = new List<int>();
        public event Action OnPostureRestored;
        private bool _wasPostureBad = false;
        private String warningMessage = "Por favor mantenha os ombros direitos";
        public event Action<string> OnWarningTriggered;
        public event Action<float> OnMovementTracked;
        public event Action OnRepetitionCompleted;

        public NeckRotationEvaluator(NeckRotationDefinition definition)
        {
            _definition = definition;
        }

        public void CalibrateOrigin(Vector3 nosePosition3D, float shoulderDiff)
        {
            _centerOrigin = new Vector2(nosePosition3D.x, nosePosition3D.y);
            _initialShoulderDiff = shoulderDiff;
            _isCalibrated = true;

            Array.Clear(_reachedQuadrants, 0, _reachedQuadrants.Length);
        }

        public void EvaluateFrame(Transform nose, Transform leftShoulder, Transform rightShoulder)
        {

            if (!_isCalibrated) return;

            float shoulderDeviation = Mathf.Abs(
                AngleCalculator.GetVerticalDifference(leftShoulder.position, rightShoulder.position)
                - _initialShoulderDiff);

            bool postureIsValid = shoulderDeviation <= _definition.shoulderAlignmentTolerance;

            if (!postureIsValid)
                OnWarningTriggered?.Invoke(warningMessage);

            float currentAmplitude = AngleCalculator.GetDistance2D(_centerOrigin, nose.position);
            if (currentAmplitude < _definition.minimumRotationAmplitude) return;

            float currentAngle = AngleCalculator.CalculateAngle360(_centerOrigin, nose.position);
            OnMovementTracked?.Invoke(currentAngle);

            if (postureIsValid)
                TrackQuadrant(currentAngle);

            if (!postureIsValid && !_wasPostureBad)
                OnWarningTriggered?.Invoke(warningMessage);
            else if (postureIsValid && _wasPostureBad)
                OnPostureRestored?.Invoke();

            _wasPostureBad = !postureIsValid;
        }

        private void TrackQuadrant(float angle)
        {
            int quadrant = Mathf.FloorToInt(angle / 90f) % 4;

            int expectedNext = _quadrantSequence.Count == 0 ? quadrant : (_quadrantSequence[^1] + 1) % 4;

            if (quadrant == expectedNext &&
                (_quadrantSequence.Count == 0 || quadrant != _quadrantSequence[0]))
            {
                _quadrantSequence.Add(quadrant);
            }

            if (_quadrantSequence.Count == 4)
            {
                OnRepetitionCompleted?.Invoke();
                _quadrantSequence.Clear();
            }
        }
    }
}
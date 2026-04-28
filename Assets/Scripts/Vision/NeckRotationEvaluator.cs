using UnityEngine;
using App.Data.ScriptableObjects;
using System;
using System.Collections.Generic;

namespace App.Vision
{
    public class NeckRotationEvaluator
    {
        private readonly NeckRotationDefinition _definition;
        private Vector3 _centerOrigin3D;
        private float _initialShoulderDiffY;
        private float _initialShoulderDiffZ;
        private bool _isCalibrated = false;
        private bool[] _reachedQuadrants = new bool[4];
        private List<int> _quadrantSequence = new List<int>();
        public event Action OnPostureRestored;
        private bool _wasPostureBad = false;
        private String warningMessageTilt = "Por favor mantenha os ombros direitos";
        private String warningMessageTwist = "Por favor mantenha o torso virado para a frente";
        public event Action<string> OnWarningTriggered;
        public event Action<float> OnMovementTracked;
        public event Action OnRepetitionCompleted;

        public NeckRotationEvaluator(NeckRotationDefinition definition)
        {
            _definition = definition;
        }

        public void CalibrateOrigin(Vector3 nosePosition, Vector3 leftShoulder, Vector3 rightShoulder)
        {
            _centerOrigin3D = nosePosition;
            _initialShoulderDiffY = AngleCalculator.GetVerticalDifference(leftShoulder, rightShoulder);
            _initialShoulderDiffZ = AngleCalculator.GetDepthDifference(leftShoulder, rightShoulder);
            _isCalibrated = true;

            Array.Clear(_reachedQuadrants, 0, _reachedQuadrants.Length);
        }

        public void EvaluateFrame(Transform nose, Transform leftShoulder, Transform rightShoulder)
        {
            if (!_isCalibrated) return;

            // Posture Validation (Y axis for tilt, Z axis for twist)
            float currentDiffY = AngleCalculator.GetVerticalDifference(leftShoulder.position, rightShoulder.position);
            float currentDiffZ = AngleCalculator.GetDepthDifference(leftShoulder.position, rightShoulder.position);

            float shoulderDeviationY = Mathf.Abs(currentDiffY - _initialShoulderDiffY);
            float shoulderDeviationZ = Mathf.Abs(currentDiffZ - _initialShoulderDiffZ);

            bool isTiltValid = shoulderDeviationY <= _definition.shoulderAlignmentTolerance;
            bool isTwistValid = shoulderDeviationZ <= _definition.shoulderTwistTolerance;

            bool postureIsValid = isTiltValid && isTwistValid;

            if (!postureIsValid && !_wasPostureBad)
            {
                string warningMsg = !isTiltValid ? warningMessageTilt : warningMessageTwist;
                OnWarningTriggered?.Invoke(warningMsg);
            }
            else if (postureIsValid && _wasPostureBad)
            {
                OnPostureRestored?.Invoke();
            }

            _wasPostureBad = !postureIsValid;

            // Amplitude Validation in pure 3D space
            float currentAmplitude3D = AngleCalculator.GetDistance3D(_centerOrigin3D, nose.position);
            if (currentAmplitude3D < _definition.minimumRotationAmplitude) return;

            // Movement Tracking
            float currentAngle = AngleCalculator.CalculateAngle360(_centerOrigin3D, nose.position);
            OnMovementTracked?.Invoke(currentAngle);

            if (postureIsValid)
            {
                TrackQuadrant(currentAngle);
            }
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
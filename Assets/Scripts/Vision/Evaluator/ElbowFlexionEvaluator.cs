using UnityEngine;
using System;
using App.Data.ScriptableObjects;

namespace App.Vision.Evaluators
{
    public class ElbowFlexionEvaluator
    {
        private readonly ElbowFlexionDefinition _definition;

        public event Action<string> OnWarningTriggered;
        public event Action OnPostureRestored;
        public event Action OnRepetitionCompleted;

        public Vector3 WristAtRest { get; private set; }
        public Vector3 WristAtPeak { get; private set; }
        private enum FlexionState { Idle, AtRest, MovingUp, AtPeak, MovingDown }
        private FlexionState _state = FlexionState.Idle;
        private bool _isWarningActive = false;
        private float _horizontalTolerance;

        public ElbowFlexionEvaluator(ElbowFlexionDefinition def)
        {
            _definition = def;
        }

        // Called once on calibration button press
        public void CalibrateAndBegin(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos,
                                      float shoulderWidth)
        {
            _horizontalTolerance = shoulderWidth * 0.6f;
            WristAtRest = wristPos;

            // Estimate the peak wrist position by rotating the rest vector
            // around the elbow by (restAngle - peakAngle).
            // This gives the visualizer a real arc end even before the user
            // has physically reached peak — they see the target, not just current pos.
            Vector2 toWrist = new Vector2(wristPos.x - elbowPos.x, wristPos.y - elbowPos.y);
            float restAngle = Mathf.Atan2(toWrist.y, toWrist.x) * Mathf.Rad2Deg;
            float peakAngle = restAngle + (_definition.isRightArm ? _definition.expectedRomDegrees
                                                       : -_definition.expectedRomDegrees);

            float rad = peakAngle * Mathf.Deg2Rad;
            float armLen = toWrist.magnitude;
            WristAtPeak = new Vector3(
                elbowPos.x + Mathf.Cos(rad) * armLen,
                elbowPos.y + Mathf.Sin(rad) * armLen,
                wristPos.z);

            _state = FlexionState.AtRest;
        }

        /*         public void EvaluateFrame(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos,
                                          out float progress)
                {
                    progress = 0f;
                    if (_state == FlexionState.Idle) return;

                    //            if (!ValidatePosture(shoulderPos, wristPos)) return;

                    float angle = AngleCalculator.CalculateJointAngle(shoulderPos, elbowPos, wristPos);

                    // Temporary — remove once reps are counting
                    if (Time.frameCount % 30 == 0)
                        Debug.Log($"[ElbowEval] angle={angle:F1} state={_state} " +
                                  $"peak<{_definition.peakAngleThreshold} " +
                                  $"rest>{_definition.restAngleThreshold}");
                    if (Time.frameCount % 30 == 0)
                        Debug.Log($"[ElbowEval] angle={angle:F1} state={_state} " +
                                  $"horizontalDev={Mathf.Abs(wristPos.x - shoulderPos.x):F1} " +
                                  $"tolerance={_definition.horizontalTolerance}");

                    progress = Mathf.Clamp01(
                        1f - Mathf.InverseLerp(_definition.peakAngleThreshold,
                                               _definition.restAngleThreshold, angle));

                    switch (_state)
                    {
                        case FlexionState.AtRest:
                            if (angle <= _definition.peakAngleThreshold)
                                _state = FlexionState.AtPeak;
                            break;
                        case FlexionState.AtPeak:
                            if (angle >= _definition.restAngleThreshold)
                            {
                                OnRepetitionCompleted?.Invoke();
                                _state = FlexionState.AtRest;
                            }
                            break;
                    }
                } */

        public void EvaluateFrame(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos,
                          out float progress)
        {
            progress = 0f;
            if (_state == FlexionState.Idle) return;

            if (!ValidatePosture(shoulderPos, wristPos)) return;

            float angle = AngleCalculator.CalculateJointAngle(shoulderPos, elbowPos, wristPos);

            if (Time.frameCount % 30 == 0)
                Debug.Log($"[ElbowEval] angle={angle:F1} state={_state}");

            progress = Mathf.Clamp01(
                1f - Mathf.InverseLerp(_definition.peakAngleThreshold,
                                       _definition.restAngleThreshold, angle));

            // Hysteresis margins — prevents boundary oscillation from firing false reps
            float peakEnterAngle = _definition.peakAngleThreshold;        // e.g. 60°
            float peakExitAngle = _definition.peakAngleThreshold + 15f;  // e.g. 75°
            float restEnterAngle = _definition.restAngleThreshold;        // e.g. 150°
            float restExitAngle = _definition.restAngleThreshold - 15f;  // e.g. 135°

            switch (_state)
            {
                case FlexionState.AtRest:
                    // Must curl past peakEnterAngle to leave rest
                    if (angle <= peakEnterAngle)
                        _state = FlexionState.MovingUp;
                    break;

                case FlexionState.MovingUp:
                    // If they start lowering before reaching peak, follow them back
                    if (angle >= restEnterAngle)
                        _state = FlexionState.AtRest;
                    // Reached peak
                    if (angle <= peakEnterAngle)
                        _state = FlexionState.AtPeak;
                    break;

                case FlexionState.AtPeak:
                    // Must exit peak zone before counting as lowering
                    if (angle >= peakExitAngle)
                        _state = FlexionState.MovingDown;
                    break;

                case FlexionState.MovingDown:
                    // Must reach full rest before counting the rep
                    if (angle >= restEnterAngle)
                    {
                        OnRepetitionCompleted?.Invoke();
                        _state = FlexionState.AtRest;
                    }
                    // If they curl back up mid-lowering, follow them
                    if (angle <= peakExitAngle)
                        _state = FlexionState.AtPeak;
                    break;
            }
        }

        private bool ValidatePosture(Vector3 shoulderPos, Vector3 wristPos)
        {
            float horizontalDeviation = Mathf.Abs(wristPos.x - shoulderPos.x);
            bool isDrifting = horizontalDeviation > _horizontalTolerance;

            if (isDrifting)
            {
                if (!_isWarningActive)
                {
                    OnWarningTriggered?.Invoke("Mantenha o cotovelo próximo ao corpo.");
                    _isWarningActive = true;
                }
                return false;
            }

            if (_isWarningActive)
            {
                OnPostureRestored?.Invoke();
                _isWarningActive = false;
            }
            return true;
        }
    }
}
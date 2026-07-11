using UnityEngine;
using System;
using App.Data.ScriptableObjects;
using App.Core;

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
        private float _peakEntryTime = -1f;
        private float _minCycleSeconds = 0.8f;


        public ElbowFlexionEvaluator(ElbowFlexionDefinition def)
        {
            _definition = def;

        }

        // Called once on calibration button press
        public void CalibrateAndBegin(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos,
                                      float shoulderWidth, bool isLeftTarget)
        {
            _state = FlexionState.AtRest;
            _horizontalTolerance = shoulderWidth * 0.6f;
            WristAtRest = wristPos;

            // Estimate the peak wrist position by rotating the rest vector
            // around the elbow by (restAngle - peakAngle).
            // This gives the visualizer a real arc end even before the user
            // has physically reached peak — they see the target, not just current pos.
            Vector2 toWrist = new Vector2(wristPos.x - elbowPos.x, wristPos.y - elbowPos.y);
            float restAngle = Mathf.Atan2(toWrist.y, toWrist.x) * Mathf.Rad2Deg;
            float peakAngle = restAngle + (isLeftTarget ? _definition.expectedRomDegrees
                                                       : -_definition.expectedRomDegrees);

            float rad = peakAngle * Mathf.Deg2Rad;
            float armLen = toWrist.magnitude;
            WristAtPeak = new Vector3(
                elbowPos.x + Mathf.Cos(rad) * armLen,
                elbowPos.y + Mathf.Sin(rad) * armLen,
                wristPos.z);
        }

        public void EvaluateFrame(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos,
                          out float progress)
        {
            progress = 0f;
            if (_state == FlexionState.Idle) return;
            if (!ValidatePosture(shoulderPos, wristPos)) return;

            float angle = AngleCalculator.CalculateJointAngle(shoulderPos, elbowPos, wristPos);

            if (SessionContext.debugMode && Time.frameCount % 30 == 0)
                Debug.Log($"[ElbowEval] angle={angle:F1} state={_state}");

            progress = Mathf.Clamp01(
                1f - Mathf.InverseLerp(_definition.peakAngleThreshold,
                                       _definition.restAngleThreshold, angle));

            float peakEnter = _definition.peakAngleThreshold;        // 60°
            float peakExit = _definition.peakAngleThreshold + 15f;  // 75°
            float restEnter = _definition.restAngleThreshold;        // 150°

            switch (_state)
            {
                case FlexionState.AtRest:
                    if (angle <= peakEnter)
                    {
                        _state = FlexionState.MovingUp;
                        _peakEntryTime = -1f;
                    }
                    break;

                case FlexionState.MovingUp:
                    if (angle >= restEnter)
                        _state = FlexionState.AtRest;
                    if (angle <= peakEnter)
                    {
                        _state = FlexionState.AtPeak;
                        _peakEntryTime = Time.time;
                    }
                    break;

                case FlexionState.AtPeak:
                    // Must stay near peak for at least one frame before lowering counts
                    if (angle >= peakExit)
                        _state = FlexionState.MovingDown;
                    break;

                case FlexionState.MovingDown:
                    if (angle <= peakExit)
                    {
                        _state = FlexionState.AtPeak; // reversed mid-lowering
                        break;
                    }
                    if (angle >= restEnter)
                    {
                        // Time gate: reject reps that complete too fast (likely noise)
                        float cycleTime = _peakEntryTime >= 0f
                            ? Time.time - _peakEntryTime
                            : float.MaxValue;

                        if (cycleTime >= _minCycleSeconds)
                        {
                            OnRepetitionCompleted?.Invoke();
                            if (SessionContext.debugMode)
                                Debug.Log($"[ElbowEval] Rep counted! cycle={cycleTime:F2}s");
                        }
                        else
                        {
                            if (SessionContext.debugMode)
                                Debug.Log($"[ElbowEval] Rep rejected: too fast ({cycleTime:F2}s < {_minCycleSeconds}s)");
                        }
                        _state = FlexionState.AtRest;
                    }
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
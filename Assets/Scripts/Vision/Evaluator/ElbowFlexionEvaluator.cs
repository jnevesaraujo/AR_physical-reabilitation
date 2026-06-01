using UnityEngine;
using System;
using App.Data.ScriptableObjects;

namespace App.Vision.Evaluators
{
    public class ElbowFlexionEvaluator
    {
        private readonly ElbowFlexionDefinition _definition;

        // Events for UI and AR Visualizer communication
        public event Action<string> OnWarningTriggered;
        public event Action OnPostureRestored;
        public event Action OnCalibrationReady;      // Fires after calibration — "prepare to move" prompt
        public event Action OnDiscoveryCompleted;    // Fires after discovery phase
        public event Action<float> OnAngleTracked;   // Tracks current elbow angle
        public event Action OnRepetitionCompleted;

        // State machine for flexion exercise
        public enum FlexionState { Idle, Discovering, Extended, Flexed, Holding }
        private FlexionState _currentState = FlexionState.Idle;

        // Posture and movement tracking
        private bool _isWarningActive = false;
        private float _holdTimer = 0f;
        private float _lastTrackedAngle = 0f;

        // Tolerance thresholds for posture validation
        private const float ShoulderHeightTolerance = 0.1f;      // Shoulder height alignment tolerance
        private const float ElbowForwardTolerance = 0.15f;       // Elbow forward/backward drift tolerance
        private const float HorizontalDriftTolerance = 0.2f;     // Wrist horizontal drift tolerance

        public float LastTrackedAngle => _lastTrackedAngle;
        public FlexionState CurrentState => _currentState;

        public ElbowFlexionEvaluator(ElbowFlexionDefinition def)
        {
            _definition = def;
        }

        /// <summary>
        /// Step 1 — Called when person taps calibrate with arm extended at rest
        /// </summary>
        public void CalibrateBaseline(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos)
        {
            _currentState = FlexionState.Discovering;
            _holdTimer = 0f;
            _isWarningActive = false;

            // Calculate initial angle to validate calibration
            float initialAngle = AngleCalculator.CalculateJointAngle3D(elbowPos, shoulderPos, wristPos);
            Debug.Log($"[ElbowFlexion] Calibrated with angle: {initialAngle:F1}°");

            // UI should now prompt: "Move your arm naturally through its range"
            OnCalibrationReady?.Invoke();
        }

        /// <summary>
        /// Main evaluation frame — called every frame during exercise
        /// </summary>
        public void EvaluateFrame(
            Vector3 shoulderPos,
            Vector3 elbowPos,
            Vector3 wristPos,
            out float progress,
            out bool isDiscovering)
        {
            progress = 0f;
            isDiscovering = (_currentState == FlexionState.Discovering);

            if (_currentState == FlexionState.Idle) return;

            // Calculate current elbow angle
            float currentAngle = AngleCalculator.CalculateJointAngle3D(elbowPos, shoulderPos, wristPos);
            _lastTrackedAngle = currentAngle;
            OnAngleTracked?.Invoke(currentAngle);

            // During discovery phase, just track movement without validating posture
            if (_currentState == FlexionState.Discovering)
            {
                return;
            }

            // Posture validation blocks rep counting but not discovery
            if (!ValidatePosture(shoulderPos, elbowPos, wristPos)) return;

            EvaluateRepetition(currentAngle, out progress);
        }

        /// <summary>
        /// Called when discovery phase completes (user confirms they're ready)
        /// </summary>
        public void ConfirmDiscovery()
        {
            if (_currentState != FlexionState.Discovering) return;

            _currentState = FlexionState.Extended;
            Debug.Log("[ElbowFlexion] Discovery completed. Exercise begins.");
            OnDiscoveryCompleted?.Invoke();
        }

        /// <summary>
        /// Validates posture to ensure proper form
        /// </summary>
        private bool ValidatePosture(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos)
        {
            // Check 1: Shoulder alignment (left and right shoulders level)
            // Note: This is simplified. In a full implementation, you'd track both shoulders
            
            // Check 2: Elbow alignment — should stay relatively close to torso
            // Measure horizontal deviation of elbow from shoulder
            float elbowDrift = Mathf.Abs(elbowPos.x - shoulderPos.x);
            bool elbowDriftValid = elbowDrift <= ElbowForwardTolerance;

            // Check 3: Wrist alignment — shouldn't drift too much horizontally
            float wristDrift = Mathf.Abs(wristPos.x - shoulderPos.x);
            bool wristDriftValid = wristDrift <= HorizontalDriftTolerance;

            if (!elbowDriftValid || !wristDriftValid)
            {
                if (!_isWarningActive)
                {
                    string msg = !elbowDriftValid
                        ? "Mantenha o cotovelo próximo ao seu corpo."
                        : "Mantenha a mão alinhada com o ombro.";
                    OnWarningTriggered?.Invoke(msg);
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

        /// <summary>
        /// Evaluates the flexion movement and repetition counting
        /// </summary>
        private void EvaluateRepetition(float currentAngle, out float progress)
        {
            progress = 0f;

            // State: Extended arm — waiting to flex
            if (_currentState == FlexionState.Extended)
            {
                progress = Mathf.Clamp01((currentAngle - _definition.flexionTargetAngle) 
                    / (_definition.extensionThresholdAngle - _definition.flexionTargetAngle));

                // Transition to flexed when angle goes below target
                if (currentAngle <= _definition.flexionTargetAngle)
                {
                    _currentState = FlexionState.Holding;
                    _holdTimer = 0f;
                    Debug.Log($"[ElbowFlexion] Flexion detected at {currentAngle:F1}°. Starting hold timer...");
                }
            }
            // State: Holding flexion — accumulate hold time
            else if (_currentState == FlexionState.Holding)
            {
                progress = Mathf.Clamp01(_holdTimer / _definition.holdTimeSeconds);

                // Keep hold if angle remains valid
                if (currentAngle <= _definition.flexionTargetAngle)
                {
                    _holdTimer += Time.deltaTime;

                    // Check if hold time is complete
                    if (_holdTimer >= _definition.holdTimeSeconds)
                    {
                        Debug.Log($"[ElbowFlexion] Hold completed. Repetition counted!");
                        OnRepetitionCompleted?.Invoke();

                        _currentState = FlexionState.Extended;
                        _holdTimer = 0f;
                    }
                }
                else
                {
                    // Lost flexion — return to extended state
                    _currentState = FlexionState.Extended;
                    _holdTimer = 0f;
                    OnWarningTriggered?.Invoke("Flexão perdida. Mantenha o braço flexionado.");
                    progress = 0f;
                }
            }
        }

        /// <summary>
        /// Calculates the elbow angle using the shoulder, elbow, and wrist positions
        /// Uses the law of cosines in 3D space
        /// </summary>
/*         private float CalculateElbowAngle(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos)
        {
            // Vectors from elbow to shoulder and elbow to wrist
            Vector3 toShoulder = (shoulderPos - elbowPos).normalized;
            Vector3 toWrist = (wristPos - elbowPos).normalized;

            // Calculate angle between the two vectors
            float dotProduct = Vector3.Dot(toShoulder, toWrist);
            float angleRadians = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f));
            float angleDegrees = angleRadians * Mathf.Rad2Deg;

            return angleDegrees;
        } */

        /// <summary>
        /// Resets the evaluator to idle state
        /// </summary>
        public void Reset()
        {
            _currentState = FlexionState.Idle;
            _holdTimer = 0f;
            _isWarningActive = false;
            _lastTrackedAngle = 0f;
        }
    }
}

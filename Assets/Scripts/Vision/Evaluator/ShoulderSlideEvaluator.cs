using UnityEngine;
using System;
using App.Data.ScriptableObjects;

namespace App.Vision.Evaluators
{
    public class ShoulderSlideEvaluator
    {
        private readonly ShoulderSlideDefinition _definition;

        public event Action<string> OnWarningTriggered;
        public event Action OnPostureRestored;
        public event Action OnCalibrationReady;   // fires after step 1 — show "raise arm" prompt
        public event Action OnDiscoveryCompleted; // fires after step 2 — exercise begins
        public event Action OnRepetitionCompleted;

        private enum SlideState { Idle, Discovering, MovingUp, MovingDown }
        private SlideState _currentState = SlideState.Idle;

        private float _startX;
        private float _startY;
        private float _maxY;
        private bool _isWarningActive = false;

        public float StartX => _startX;
        public float StartY => _startY;
        public float MaxY => _maxY;

        public ShoulderSlideEvaluator(ShoulderSlideDefinition def)
        {
            _definition = def;
        }

        // Step 1 — called when person taps calibrate with arm resting
        public void CalibrateBaseline(Vector3 wristPos)
        {
            _startX = wristPos.x;
            _startY = wristPos.y;
            _maxY = _startY;
            _isWarningActive = false;
            _currentState = SlideState.Discovering;

            // UI should now prompt: "Raise your arm as high as comfortable, then confirm"
            OnCalibrationReady?.Invoke();
        }

        // Step 2 — called when person taps confirm at their maximum height
        public void ConfirmPeak(Vector3 wristPos)
        {
            if (_currentState != SlideState.Discovering) return;

            // Use the higher of: current wrist pos or any peak tracked during raising
            _maxY = Mathf.Max(_maxY, wristPos.y);

            float rom = _maxY - _startY;
            Debug.Log($"[ShoulderSlide] ROM locked: {rom:F3} units");

            _currentState = SlideState.MovingUp;
            OnDiscoveryCompleted?.Invoke();
        }

        public void EvaluateFrame(
            Vector3 shoulderPos,
            Vector3 elbowPos,
            Vector3 wristPos,
            out float progress,
            out bool isDiscovering)
        {
            progress = 0f;
            isDiscovering = (_currentState == SlideState.Discovering);

            if (_currentState == SlideState.Idle) return;

            // During discovery, track peak in case person doesn't hold perfectly still
            if (_currentState == SlideState.Discovering)
            {
                if (wristPos.y > _maxY) _maxY = wristPos.y;
                return;
            }

            // Posture check blocks rep counting but not discovery
            if (!ValidatePosture(shoulderPos, elbowPos, wristPos)) return;

            EvaluateRepetition(wristPos, out progress);
        }

        private bool ValidatePosture(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos)
        {
            bool elbowTooHigh = elbowPos.y > shoulderPos.y;
            float horizontalDeviation = Mathf.Abs(wristPos.x - _startX);
            bool isDrifting = horizontalDeviation > _definition.horizontalTolerance;

            if (elbowTooHigh || isDrifting)
            {
                if (!_isWarningActive)
                {
                    string msg = elbowTooHigh
                        ? "Elbow too high. Lower it slightly."
                        : "Keep your hand aligned. Don't drift sideways!";
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

        private void EvaluateRepetition(Vector3 wristPos, out float progress)
        {
            float rom = _maxY - _startY;

            if (rom < 0.05f)
            {
                progress = 0f;
                return;
            }

            progress = Mathf.Clamp01((wristPos.y - _startY) / rom);

            if (_currentState == SlideState.MovingUp && progress >= 0.85f)
            {
                _currentState = SlideState.MovingDown;
            }
            else if (_currentState == SlideState.MovingDown && progress <= 0.15f)
            {
                OnRepetitionCompleted?.Invoke();
                _currentState = SlideState.MovingUp;
            }
        }
    }
}
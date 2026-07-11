using UnityEngine;
using System;
using App.Data.ScriptableObjects;
using Unity.VisualScripting;

namespace App.Vision.Evaluators
{
    public class HandGripEvaluator
    {
        private HandGripDefinition _definition;

        private Vector3 _centerOrigin3D;

        // Communication events for external systems to respond to evaluation results
        public event Action<string> OnWarningTriggered;
        public event Action OnPostureRestored;
        public event Action OnRepetitionCompleted;
        // Máquina de estados da contração isométrica
        private bool _isHolding = false;
        private float _holdTimer = 0f;
        private bool _requiresRelease = false;
        private float _calibratedMaxAperture = 0.01f;

        private float _logCooldown = 0f;

        public HandGripEvaluator(HandGripDefinition def)
        {
            _definition = def;
        }

        public void CalibrateOrigin(Vector3 thumbPos, Vector3 indexPos)
        {
            // Central point between thumb and index finger for reference
            _centerOrigin3D = (thumbPos + indexPos) / 2f;

            // Maximum distance between thumb and index finger when fully open
            float distance = Vector3.Distance(thumbPos, indexPos);
            _calibratedMaxAperture = Mathf.Max(distance, 0.01f); // Avoid division by zero and ensure a minimum threshold
        }

        public void EvaluateFrame(Vector3 thumbPos, Vector3 indexPos, out float apertureRatio, out float holdProgress)
        {
            // Default values for output parameters
            apertureRatio = 1.0f;
            holdProgress = 0.0f;

            float maxAperture = _calibratedMaxAperture;
            if (maxAperture <= 0.001f) return;

            float currentDistance = Vector3.Distance(thumbPos, indexPos);
            apertureRatio = currentDistance / maxAperture;

            // Calculate hold progress only if the user is currently holding and the defined hold time is greater than zero
            if (_isHolding && _definition.isometricHoldTime > 0)
            {
                holdProgress = Mathf.Clamp01(_holdTimer / _definition.isometricHoldTime);
            }

            _logCooldown -= Time.deltaTime;
            if (_logCooldown <= 0f)
            {
                _logCooldown = 0.5f;
            }

            // Release phase: if the hand is open beyond the release threshold, reset the state and trigger the posture restored event
            if (apertureRatio >= _definition.releaseDistance)
            {
                if (_requiresRelease)
                {
                    _requiresRelease = false;
                    OnPostureRestored?.Invoke();
                }
                
                if (_isHolding)
                {
                    _isHolding = false;
                    _holdTimer = 0f;
                    holdProgress = 0f; // Reset hold progress when the contraction is interrupted
                    OnWarningTriggered?.Invoke("Contração interrompida. Mantenha a pinça fechada.");
                }
            }
            // Contraction phase: if the hand is closed beyond the target grip distance and a release is not required, start or continue the hold timer
            else if (!_requiresRelease && apertureRatio <= _definition.targetGripDistance)
            {
                if (!_isHolding)
                {
                    _isHolding = true;
                    _holdTimer = 0f;
                }

                _holdTimer += Time.deltaTime;

                if (_holdTimer >= _definition.isometricHoldTime)
                {
                    OnRepetitionCompleted?.Invoke();
                    
                    _isHolding = false;
                    _requiresRelease = true; 
                    
                    OnWarningTriggered?.Invoke("Repetição válida. Abra a mão para continuar.");
                }
            }
            else if (!_requiresRelease && apertureRatio <= _definition.targetGripDistance)
            {
                if (!_isHolding)
                {
                    _isHolding = true;
                    _holdTimer = 0f;
                }

                _holdTimer += Time.deltaTime;

                if (_holdTimer >= _definition.isometricHoldTime)
                {
                    OnRepetitionCompleted?.Invoke();
                    
                    _isHolding = false;
                    _requiresRelease = true; 
                    
                    OnWarningTriggered?.Invoke("Repetição válida. Abra a mão para continuar.");
                }
            }
        }
    }
}
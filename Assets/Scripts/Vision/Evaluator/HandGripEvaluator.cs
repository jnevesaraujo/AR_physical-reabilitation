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

        // Eventos de comunicação com a UI e Visualizador AR
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
            // 1. Cálculo do ponto central no espaço 3D (Vector3) para posicionar o biofeedback
            _centerOrigin3D = (thumbPos + indexPos) / 2f;

            // 2. Registo da abertura máxima calibrada (float) para os rácios de contração
            float distance = Vector3.Distance(thumbPos, indexPos);
            _calibratedMaxAperture = Mathf.Max(distance, 0.01f); // Evita divisão por zero
        }

        public void EvaluateFrame(Vector3 thumbPos, Vector3 indexPos)
        {
            float maxAperture = _calibratedMaxAperture;
            
            if (maxAperture <= 0.001f) return;

            float currentDistance = Vector3.Distance(thumbPos, indexPos);
            float apertureRatio = currentDistance / maxAperture;

            _logCooldown -= Time.deltaTime;
            if (_logCooldown <= 0f)
            {
                // Remova ou comente a linha abaixo quando tiver a certeza de que os valores (0.15 e 0.85) estão corretos para a sua câmara
                Debug.Log($"[HandPinch] Rácio atual: {apertureRatio:F2} | Alvo Grip: {_definition.targetGripDistance} | Alvo Release: {_definition.releaseDistance}");
                _logCooldown = 0.5f;
            }

            // 1. Fase de Libertação (Release / Mão Aberta)
            if (apertureRatio >= _definition.releaseDistance)
            {
                if (_requiresRelease)
                {
                    Debug.Log("<color=green>[HandPinch] Mão reaberta. O ciclo foi reiniciado.</color>");
                    _requiresRelease = false;
                    OnPostureRestored?.Invoke();
                }
                
                // Se estava a segurar e abriu antes do tempo
                if (_isHolding)
                {
                    Debug.LogWarning("<color=orange>[HandPinch] Contração interrompida precocemente!</color>");
                    _isHolding = false;
                    _holdTimer = 0f;
                    OnWarningTriggered?.Invoke("Contração interrompida. Mantenha a pinça fechada.");
                }
            }
            // 2. Fase de Contração Isométrica (Pinch / Mão Fechada)
            else if (!_requiresRelease && apertureRatio <= _definition.targetGripDistance)
            {
                if (!_isHolding)
                {
                    Debug.Log("<color=cyan>[HandPinch] Pinça fechada! A iniciar temporizador...</color>");
                    _isHolding = true;
                    _holdTimer = 0f;
                }

                _holdTimer += Time.deltaTime;

                // Validação da repetição
                if (_holdTimer >= _definition.isometricHoldTime)
                {
                    Debug.Log($"<color=yellow>[HandPinch] SUCESSO! Repetição registada após {_definition.isometricHoldTime}s.</color>");
                    OnRepetitionCompleted?.Invoke();
                    
                    _isHolding = false;
                    _requiresRelease = true; // Bloqueia novas validações até reabrir a mão
                    
                    OnWarningTriggered?.Invoke("Repetição válida. Abra a mão para continuar.");
                }
            }
        }
    }
}
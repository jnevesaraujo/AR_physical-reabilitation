using UnityEngine;
using System;
using App.Data.ScriptableObjects;

namespace App.Vision.Evaluators
{
    public class ShoulderSlideEvaluator
    {
        private ShoulderSlideDefinition _definition;

        public event Action<string> OnWarningTriggered;
        public event Action OnPostureRestored;
        public event Action OnDiscoveryCompleted;
        public event Action OnRepetitionCompleted;

        private enum SlideState { Idle, Discovering, MovingDown, MovingUp }
        private SlideState _currentState = SlideState.Idle;

        // Posições de referência (Calibração e Descoberta)
        private float _startX;
        private float _startY;
        private float _maxY;
        
        // Temporizador para a descoberta da amplitude
        private float _holdTimer = 0f;
        private float _lastWristY = 0f;
        
        private bool _isWarningActive = false;

        public ShoulderSlideEvaluator(ShoulderSlideDefinition def)
        {
            _definition = def;
        }

        public void CalibrateOrigin(Vector3 wristPos)
        {
            _startX = wristPos.x; // Cria o carril virtual (linha reta)
            _startY = wristPos.y; // Base do movimento
            _maxY = _startY;      // Inicialmente, o máximo é a base
            
            _currentState = SlideState.Discovering;
            _isWarningActive = false;
        }

        public void EvaluateFrame(Vector3 shoulderPos, Vector3 elbowPos, Vector3 wristPos, out float progress, out bool isDiscovering)
        {
            progress = 0f;
            isDiscovering = (_currentState == SlideState.Discovering);

            if (_currentState == SlideState.Idle) return;

            // --- 1. Postura ---
            bool isElbowTooHigh = elbowPos.y > shoulderPos.y;
            bool isDrifting = Mathf.Abs(wristPos.x - _startX) > _definition.horizontalTolerance;

            if (isElbowTooHigh || isDrifting)
            {
                if (!_isWarningActive)
                {
                    string msg = isElbowTooHigh ? "Cotovelo demasiado alto! Baixe um pouco." : "Mantenha a mão alinhada. Não fuja para os lados!";
                    OnWarningTriggered?.Invoke(msg);
                    _isWarningActive = true;
                }
                return; // Bloqueia a progressão do exercício se a postura estiver má
            }
            else if (_isWarningActive)
            {
                OnPostureRestored?.Invoke();
                _isWarningActive = false;
            }

            // --- 2. FASE DE DESCOBERTA (Repetição Zero) ---
            if (_currentState == SlideState.Discovering)
            {
                // Se o utilizador subiu a mão mais alto, atualiza o topo e reinicia o temporizador
                if (wristPos.y > _maxY + 0.02f) 
                {
                    _maxY = wristPos.y;
                    _holdTimer = 0f;
                }
                
                // Verifica se a mão está parada perto do topo atual
                if (Mathf.Abs(wristPos.y - _maxY) < 0.05f && wristPos.y > _startY + 0.1f)
                {
                    _holdTimer += Time.deltaTime;
                    if (_holdTimer >= _definition.discoveryHoldTime)
                    {
                        Debug.Log($"[ShoulderSlide] Amplitude Registada! Subiu: {_maxY - _startY} metros.");
                        _currentState = SlideState.MovingDown; // Descoberta concluída, manda descer
                        OnDiscoveryCompleted?.Invoke();
                    }
                }
                return;
            }

            // --- 3. FASE DE EXERCÍCIO (Contagem) ---
            float currentRom = _maxY - _startY;
            if (currentRom > 0.05f) // Evita divisão por zero
            {
                progress = Mathf.Clamp01((wristPos.y - _startY) / currentRom);
            }

            if (_currentState == SlideState.MovingDown)
            {
                if (progress <= 0.15f) // Voltou à base (15% de tolerância)
                {
                    _currentState = SlideState.MovingUp;
                }
            }
            else if (_currentState == SlideState.MovingUp)
            {
                if (progress >= 0.85f) // Atingiu o topo registado (com tolerância)
                {
                    OnRepetitionCompleted?.Invoke();
                    _currentState = SlideState.MovingDown;
                }
            }
        }
    }
}
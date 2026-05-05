using UnityEngine;
using App.Data.ScriptableObjects;
using App.UI;
using App.Vision.Evaluators;

namespace App.Vision.Extractors
{
    public class ShoulderDataExtractor : MonoBehaviour
    {
        private ShoulderSlideDefinition _exerciseDef;
        private ExerciseHUD _hud;
        private ARExerciseVisualizer _visualizer;
        
        private ShoulderSlideEvaluator _evaluator;
        
        private bool _isCalibrated = false;
        private int _currentRepetitions = 0;

        // Pontos do esqueleto
        private Transform _shoulder; // Ponto 11 (Esq) ou 12 (Dir)
        private Transform _elbow;    // Ponto 13 (Esq) ou 14 (Dir)
        private Transform _wrist;    // Ponto 15 (Esq) ou 16 (Dir)

        public void Initialize(ExerciseDefinition definition, ExerciseHUD hud, ARExerciseVisualizer visualizer)
        {
            _exerciseDef = definition as ShoulderSlideDefinition;
            _hud = hud;
            _visualizer = visualizer;

            _evaluator = new ShoulderSlideEvaluator(_exerciseDef);
            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnDiscoveryCompleted += HandleDiscoveryCompleted;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;

            if (_hud != null)
            {
                _hud.OnCalibrationRequested += CalibrateAndStart;
            }
        }

        private void OnDestroy()
        {
            if (_hud != null) _hud.OnCalibrationRequested -= CalibrateAndStart;
        }

        private void Update()
        {
            if (_exerciseDef == null) return;
            
            if (_shoulder == null)
            {
                GameObject pointList = GameObject.Find("Point List Annotation");
                if (pointList == null || pointList.transform.childCount < 33) return;

                // Escolhe os pontos corretos consoante o braço escolhido no ScriptableObject
                int shoulderIdx = _exerciseDef.isRightArm ? 12 : 11;
                int elbowIdx = _exerciseDef.isRightArm ? 14 : 13;
                int wristIdx = _exerciseDef.isRightArm ? 16 : 15;

                _shoulder = pointList.transform.GetChild(shoulderIdx);
                _elbow = pointList.transform.GetChild(elbowIdx);
                _wrist = pointList.transform.GetChild(wristIdx);
            }

            if (_isCalibrated && _shoulder != null && _elbow != null && _wrist != null)
            {
                float currentProgress;
                bool isDiscovering;
                
                _evaluator.EvaluateFrame(_shoulder.position, _elbow.position, _wrist.position, out currentProgress, out isDiscovering);

                // Na versão final, podes criar um visualizador de "carril" usando estas variáveis
                // _visualizer.UpdateShoulderSlideVisuals(_wrist.position, currentProgress, isDiscovering);
            }
        }

        public void CalibrateAndStart()
        {
            if (_wrist == null) return;

            _evaluator.CalibrateOrigin(_wrist.position);
            _isCalibrated = true;
            
            // Mensagem UI para guiar a Repetição Zero
            if (_hud != null)
            {
                _hud.HideWarning();
                _hud.ShowWarning("Fase de Descoberta: Suba o braço o máximo possível sem dor e aguarde.");
            }
        }

        private void HandleDiscoveryCompleted()
        {
            if (_hud != null) 
            {
                _hud.ShowWarning("Amplitude registada! Volte a descer a mão para iniciar as repetições."); // Mostrar por 3 segundos
            }
        }

        // --- Event Handlers Normais ---
        private void HandleBadPosture(string warningMessage)
        {
            _visualizer.SetFeedbackMode(false);
            if (_hud != null) _hud.ShowWarning(warningMessage);
        }

        private void HandlePostureRestored()
        {
            _visualizer.SetFeedbackMode(true);
            if (_hud != null) _hud.HideWarning();
        }

        private void HandleRepetitionSuccess()
        {
            _currentRepetitions++;
            if (_hud != null) _hud.UpdateRepetitionCount(_currentRepetitions, _exerciseDef.targetRepetitions);
            _visualizer.TriggerSuccessFeedback();
        }
    }
}
using UnityEngine;
using App.Data.ScriptableObjects;
using App.UI;
using App.Vision.Evaluators;
// using App.Vision.Evaluators;

namespace App.Vision.Extractors
{
    public class HandDataExtractor : MonoBehaviour
    {
        private HandGripDefinition _exerciseDef;
        private ExerciseHUD _hud;
        private ARExerciseVisualizer _visualizer;

        private HandGripEvaluator _evaluator;

        private bool _isCalibrated = false;
        private int _currentRepetitions = 0;

        private Transform _thumbTip;
        private Transform _indexTip;
        private Transform _wrist;    // Ponto 0
        private Transform _indexMCP; // Ponto 5
        private Transform _pinkyMCP; // Ponto 17

        public void Initialize(ExerciseDefinition definition, ExerciseHUD hud, ARExerciseVisualizer visualizer)
        {
            // O cast assegura que temos acesso às variáveis específicas do HandGrip
            _exerciseDef = definition as HandGripDefinition;
            _hud = hud;
            _visualizer = visualizer;

            _evaluator = new HandGripEvaluator(_exerciseDef);
            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;
        }

        private void Update()
        {
            // Procura preguiçosa da hierarquia de anotação
            if (_thumbTip == null)
            {
                GameObject pointList = GameObject.Find("Point List Annotation");

                // O modelo Hand devolve 21 pontos
                if (pointList == null || pointList.transform.childCount < 21) return;

                // Extração dos Transforms (Índice 4: Polegar, Índice 8: Indicador)
                _wrist = pointList.transform.GetChild(0);
                _thumbTip = pointList.transform.GetChild(4);
                _indexMCP = pointList.transform.GetChild(5);
                _indexTip = pointList.transform.GetChild(8);
                _pinkyMCP = pointList.transform.GetChild(17);
            }

            if (_isCalibrated && _thumbTip != null && _indexTip != null)
            {
                float currentApertureRatio;
                float currentHoldProgress;
                _evaluator.EvaluateFrame(_thumbTip.position, _indexTip.position, out currentApertureRatio, out currentHoldProgress);

                Vector3 absolutePalmCenter = (_wrist.position + _indexMCP.position + _pinkyMCP.position) / 3f;

                _visualizer.UpdateHandGripVisuals(absolutePalmCenter, currentApertureRatio, currentHoldProgress);
            }
        }

        public void CalibrateAndStart()
        {
            Debug.Log("Trying to calibrate");
            if (_thumbTip == null || _indexTip == null) return;

            _evaluator.CalibrateOrigin(_thumbTip.position, _indexTip.position);
            _isCalibrated = true;

            Vector3 startPalmCenter = (_wrist.position + _indexMCP.position + _pinkyMCP.position) / 3f;
            _visualizer.InitializeGuide(_exerciseDef, startPalmCenter);
            
            if (_hud != null) _hud.HideWarning();
            Debug.Log("Calibrated");
        }

        // --- Event Handlers ---
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
        }
    }
}
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
                _thumbTip = pointList.transform.GetChild(4);
                _indexTip = pointList.transform.GetChild(8);
            }

            if (_isCalibrated && _thumbTip != null && _indexTip != null)
            {
                _visualizer.UpdatePacerFeedback(_indexTip.position);
                _evaluator.EvaluateFrame(_thumbTip.position, _indexTip.position);
            }
        }

        public void CalibrateAndStart()
        {
            Debug.Log("Trying to calibrate");
            if (_thumbTip == null || _indexTip == null) return;

            _evaluator.CalibrateOrigin(_thumbTip.position, _indexTip.position);
            _isCalibrated = true;

            _visualizer.InitializeGuide(_exerciseDef, _indexTip.position);
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
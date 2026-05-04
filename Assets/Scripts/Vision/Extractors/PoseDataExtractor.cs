using UnityEngine;
using App.Data.ScriptableObjects;
using App.UI;
using App.Vision;

namespace App.Vision
{
    public class PoseDataExtractor : MonoBehaviour
    {
        private NeckRotationDefinition _exerciseDef;
        private ExerciseHUD _hud;
        private ARExerciseVisualizer _visualizer;
        
        private NeckRotationEvaluator _evaluator;
        private bool _isCalibrated = false;
        private Transform _nose, _leftShoulder, _rightShoulder;
        private int _currentRepetitions = 0;

        // Invocado pelo ExerciseAppManager
        public void Initialize(NeckRotationDefinition definition, ExerciseHUD hud, ARExerciseVisualizer visualizer)
        {
            _exerciseDef = definition;
            _hud = hud;
            _visualizer = visualizer;

            _evaluator = new NeckRotationEvaluator(_exerciseDef);
            
            // Subscrição de eventos
            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;
        }

        private void Update()
        {
            // Procura preguiçosa (Lazy Find) dos transforms do MediaPipe
            if (_nose == null)
            {
                GameObject pointList = GameObject.Find("Point List Annotation");
                if (pointList == null || pointList.transform.childCount < 33) return;

                _nose = pointList.transform.GetChild(0);
                _leftShoulder = pointList.transform.GetChild(11);
                _rightShoulder = pointList.transform.GetChild(12);
            }

            if (_isCalibrated && _nose != null && _leftShoulder != null && _rightShoulder != null)
            {
                _visualizer.UpdatePacerFeedback(_nose.position);
                _evaluator.EvaluateFrame(_nose, _leftShoulder, _rightShoulder);
            }
        }

        public void CalibrateAndStart()
        {
            if (_nose == null || _leftShoulder == null) return;

            _evaluator.CalibrateOrigin(_nose.position, _leftShoulder.position, _rightShoulder.position);
            _isCalibrated = true;

            _visualizer.InitializeGuide(_exerciseDef, _nose.position);
            if (_hud != null) _hud.HideWarning();
        }

        // --- Eventos de Feedback ---
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
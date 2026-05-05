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
        private bool _isBaselineCaptured = false; // true after step 1, waiting for step 2
        private int _currentRepetitions = 0;

        private Transform _shoulder;
        private Transform _elbow;
        private Transform _wrist;

        public void Initialize(ExerciseDefinition definition, ExerciseHUD hud, ARExerciseVisualizer visualizer)
        {
            _exerciseDef = definition as ShoulderSlideDefinition;
            _hud = hud;
            _visualizer = visualizer;

            _evaluator = new ShoulderSlideEvaluator(_exerciseDef);
            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnCalibrationReady += HandleCalibrationReady;
            _evaluator.OnDiscoveryCompleted += HandleDiscoveryCompleted;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;

            // Step 1 — HUD calibrate button calls CalibrateBaseline
            if (_hud != null)
            {
                _hud.OnCalibrationRequested += CalibrateBaseline;
                _hud.OnPeakConfirmRequested += ConfirmPeak; // Step 2 — new HUD event
            }
        }

        private void OnDestroy()
        {
            if (_hud != null)
            {
                _hud.OnCalibrationRequested -= CalibrateBaseline;
                _hud.OnPeakConfirmRequested -= ConfirmPeak;
            }
        }

        private void Update()
        {
            if (_shoulder == null)
            {
                GameObject pointList = GameObject.Find("Point List Annotation");
                if (pointList == null || pointList.transform.childCount < 33) return;

                int shoulderIdx = _exerciseDef.isRightArm ? 12 : 11;
                int elbowIdx    = _exerciseDef.isRightArm ? 14 : 13;
                int wristIdx    = _exerciseDef.isRightArm ? 16 : 15;

                _shoulder = pointList.transform.GetChild(shoulderIdx);
                _elbow    = pointList.transform.GetChild(elbowIdx);
                _wrist    = pointList.transform.GetChild(wristIdx);
            }

            if (_isCalibrated && _shoulder != null && _elbow != null && _wrist != null)
            {
                _evaluator.EvaluateFrame(
                    _shoulder.position,
                    _elbow.position,
                    _wrist.position,
                    out float currentProgress,
                    out bool isDiscovering);
                _visualizer.UpdateShoulderSlideVisuals(_wrist.position, currentProgress, isDiscovering);
            }
        }

        // Step 1 — called by HUD calibrate button
        // Person should have arm resting at their side
        public void CalibrateBaseline()
        {
            if (_wrist == null)
            {
                return;
            }

            _evaluator.CalibrateBaseline(_wrist.position);
            _isBaselineCaptured = true;
            _isCalibrated = true;

        }

        // Step 2 — called by HUD confirm button
        // Person should have arm raised to their comfortable maximum
        public void ConfirmPeak()
        {
            if (!_isBaselineCaptured)
            {
                Debug.LogWarning("[ShoulderSlide] Não foi possível confirmar pico porque a linha de base não foi calibrada.");
                return;
            }

            if (_wrist == null) return;

            _evaluator.ConfirmPeak(_wrist.position);
            Vector3 startPos = new Vector3(_evaluator.StartX, _evaluator.StartY, _wrist.position.z);
            _visualizer.InitializeGuide(_exerciseDef, startPos, _evaluator.MaxY);
            // OnDiscoveryCompleted event will handle the UI transition
        }

        // --- Event Handlers ---

        // Fires after step 1 — prompt user to raise arm
        private void HandleCalibrationReady()
        {

            if (_hud != null)
            {
                _hud.HideWarning();
                _hud.ShowConfirmPeakButton(); // show the step 2 button
                _hud.ShowWarning("Levante o seu braço até onde for confortável, depois confirme.");
            }
        }

        // Fires after step 2 — ROM locked, exercise begins
        private void HandleDiscoveryCompleted()
        {
            if (_hud != null)
            {
                _hud.HideConfirmPeakButton(); // hide step 2 button
                _hud.HideWarning();
                _hud.ShowWarning("Agora comece o exercicio.");
            }
        }

        private void HandleBadPosture(string warningMessage)    
        {
            if (_visualizer != null) _visualizer.SetFeedbackMode(false);
            if (_hud != null) _hud.ShowWarning(warningMessage);
        }

        private void HandlePostureRestored()
        {
            if (_visualizer != null) _visualizer.SetFeedbackMode(true);
            if (_hud != null) _hud.HideWarning();
        }

        private void HandleRepetitionSuccess()
        {
            _currentRepetitions++;
            if (_hud != null)
                _hud.UpdateRepetitionCount(_currentRepetitions, _exerciseDef.targetRepetitions);
            if (_visualizer != null)
                _visualizer.TriggerSuccessFeedback();
        }
    }
}
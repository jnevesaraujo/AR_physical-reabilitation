using UnityEngine;
using App.Data.ScriptableObjects;
using App.UI;
using App.Vision.Guides;

namespace App.Vision.Extractors
{
    public class NeckRotationExtractor : BaseExerciseExtractor
    {
        private NeckRotationEvaluator _evaluator;

        // Neck-specific landmarks
        private Transform _nose;
        private Transform _leftShoulder;
        private Transform _rightShoulder;

        protected override void OnInitialize()
        {
            _evaluator = new NeckRotationEvaluator(_exerciseDef as NeckRotationDefinition);

            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;
        }

        protected override void CalibrateAndStart()
        {
            if (_nose == null || _leftShoulder == null || _rightShoulder == null) return;

            _evaluator.CalibrateOrigin(_nose.position, _leftShoulder.position, _rightShoulder.position);
            _isCalibrated = true;

            float shoulderWidth = Vector2.Distance(
                new Vector2(_leftShoulder.position.x, _leftShoulder.position.y),
                new Vector2(_rightShoulder.position.x, _rightShoulder.position.y));
            float visualRadius = shoulderWidth * 0.2f;
            Debug.Log($"[NeckRotation] Calibrated with shoulderWidth={shoulderWidth:F1}px, visualRadius={visualRadius:F1}px");

            // Tell the guide its pacer speed before Initialize fires
            var def = _exerciseDef as NeckRotationDefinition;
            _visualizer.InitializeGuide(_exerciseDef, _nose.position, visualRadius);

            // After InitializeGuide the guide exists — cast and set speed
            // Alternatively use a dedicated visualizer helper; this is the simplest path
            if (def != null)
            {
                var guide = FindFirstObjectByType<NeckGuide>();
                guide?.SetPacerSpeed(def.targetSecondsPerRep);
            }

            if (_hud != null) _hud.HideWarning();
        }
        protected override void OnEvaluateFrame()
        {
            // Cache neck-specific landmarks from the shared _pointList
            if (_nose == null)
            {
                _nose = _pointList.GetChild(0);
                _leftShoulder = _pointList.GetChild(11);
                _rightShoulder = _pointList.GetChild(12);
            }

            if (!_isCalibrated) return;

            _evaluator.EvaluateFrame(_nose, _leftShoulder, _rightShoulder);
        }

        protected override void OnSessionComplete()
        {
            Debug.Log("[NeckRotation] Session complete.");
            // navigate to results screen
        }
    }
}
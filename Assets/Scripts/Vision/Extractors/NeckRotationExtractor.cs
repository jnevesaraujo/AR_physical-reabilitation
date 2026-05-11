using UnityEngine;
using App.Data.ScriptableObjects;
using App.UI;

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
            if (_nose == null || _leftShoulder == null || _rightShoulder == null)
            {
                Debug.LogWarning("[NeckRotation] Cannot calibrate: landmarks not detected yet.");
                return;
            }

            _evaluator.CalibrateOrigin(_nose.position, _leftShoulder.position, _rightShoulder.position);
            _isCalibrated = true;

            _visualizer.InitializeGuide(_exerciseDef, _nose.position);

            if (_hud != null) _hud.HideWarning();
        }

        protected override void OnEvaluateFrame()
        {
            // Cache neck-specific landmarks from the shared _pointList
            if (_nose == null)
            {
                _nose          = _pointList.GetChild(0);
                _leftShoulder  = _pointList.GetChild(11);
                _rightShoulder = _pointList.GetChild(12);
            }

            if (!_isCalibrated) return;

            _visualizer.UpdatePacerFeedback(_nose.position);
            _evaluator.EvaluateFrame(_nose, _leftShoulder, _rightShoulder);
        }

        protected override void OnSessionComplete()
        {
            Debug.Log("[NeckRotation] Session complete.");
            // navigate to results screen
        }
    }
}
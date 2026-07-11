using UnityEngine;
using App.Data.ScriptableObjects;
using App.Vision.Evaluators;
using App.Vision.Guides;

namespace App.Vision.Extractors
{
    public class HandGripExtractor : BaseExerciseExtractor
    {
        private HandGripEvaluator _evaluator;
        protected override int RequiredLandmarkCount => 21;

        // Hand-specific landmarks (21 points, different annotation object)
        private Transform _wrist;
        private Transform _thumbTip;
        private Transform _indexTip;
        private Transform _indexMCP;
        private Transform _pinkyMCP;

        // Palm center derived each frame from 3 landmarks
        private Vector3 PalmCenter => (_wrist.position + _indexMCP.position + _pinkyMCP.position) / 3f;

        protected override void OnInitialize()
        {
            _evaluator = new HandGripEvaluator(_exerciseDef as HandGripDefinition);

            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;
        }

        protected override void CalibrateAndStart()
        {
            ResetLandmarkFilters();
            if (_thumbTip == null || _indexTip == null || _wrist == null)
            {
                Debug.LogWarning("[HandGrip] Cannot calibrate: hand landmarks not detected yet.");
                return;
            }

            _evaluator.CalibrateOrigin(_thumbTip.position, _indexTip.position);
            _isCalibrated = true;

            float handScale = Vector3.Distance(_thumbTip.position, _indexTip.position);
            _visualizer.InitializeGuide(_exerciseDef, PalmCenter, handScale);

            if (_hud != null) _hud.HideWarning();
            Debug.Log("[HandGrip] Calibrated.");
        }

        protected override void OnEvaluateFrame()
        {
            if (_thumbTip == null)
            {
                GameObject handList = GameObject.Find("Point List Annotation");
                if (handList == null) return;

                if (handList.transform.childCount < 21) return;

                _wrist = handList.transform.GetChild(0);
                _thumbTip = handList.transform.GetChild(4);
                _indexMCP = handList.transform.GetChild(5);
                _indexTip = handList.transform.GetChild(8);
                _pinkyMCP = handList.transform.GetChild(17);
            }

            if (!_isCalibrated) return;

            Vector3 thumbPos = GetFilteredLandmark(4);
            Vector3 indexPos = GetFilteredLandmark(8);

            _evaluator.EvaluateFrame(thumbPos, indexPos, out float currentApertureRatio, out float currentHoldProgress);

            _visualizer.UpdateHandGripVisuals(PalmCenter, currentApertureRatio, currentHoldProgress);
        }

        protected override void OnSessionComplete()
        {
            Debug.Log("[HandGrip] Session complete.");
            // navigate to results screen
        }
    }
}
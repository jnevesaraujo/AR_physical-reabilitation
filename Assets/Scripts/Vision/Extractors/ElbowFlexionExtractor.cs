using UnityEngine;
using App.Data.ScriptableObjects;
using App.Vision.Evaluators;
using System.Collections.Generic;
using App.Core;
using App.Data.Models;

namespace App.Vision.Extractors
{
    public class ElbowFlexionExtractor : BaseExerciseExtractor
    {
        private ElbowFlexionEvaluator _evaluator;
        private ElbowFlexionDefinition _def;
        private Transform _shoulder, _elbow, _wrist;
        private bool _peakConfirmed = false;
        private bool _isLeftTarget;
        private Queue<Vector3> _wristBuffer = new Queue<Vector3>();
        private const int BufferSize = 8;
        private float _armLength = 1f;

        protected override void OnInitialize()
        {
            _def = _exerciseDef as ElbowFlexionDefinition;
            _evaluator = new ElbowFlexionEvaluator(_def);
            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;

            _isLeftTarget = (SessionContext.CurrentUser?.affectedSide ?? AffectedSide.Unknown) == AffectedSide.Left;

            // Wire peak confirm — same pattern as ShoulderSlide
            if (_hud != null)
                _hud.OnPeakConfirmed += HandlePeakConfirm;

            // in ElbowFlexionExtractor.OnInitialize()
            Debug.Log($"[ElbowFlexion] CurrentUser={SessionContext.CurrentUser?.userId} " +
                      $"affectedSide={SessionContext.CurrentUser?.affectedSide} isLeftTarget={_isLeftTarget}");

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_hud != null)
                _hud.OnPeakConfirmed -= HandlePeakConfirm;
        }

        // Step 1 — patient taps calibrate with arm at rest
        protected override void CalibrateAndStart()
        {
            ResetLandmarkFilters();
            if (_shoulder == null || _elbow == null || _wrist == null) return;
            if (_wristBuffer.Count == 0)
            {
                _hud?.ShowWarning("Aguarde a deteção do corpo...");
                return;
            }

            Vector3 restPos = SmoothedWristPos;
            if (float.IsNaN(restPos.x)) return;

            float upperArmLength = Vector3.Distance(_shoulder.position, _elbow.position);
            float foreArmLength = Vector3.Distance(_elbow.position, _wrist.position);

            _armLength = Mathf.Max(upperArmLength, foreArmLength);

            // in ElbowFlexionExtractor.CalibrateAndStart(), before the early-return check
            Debug.Log($"[ElbowFlexion] shoulder={_shoulder} elbow={_elbow} wrist={_wrist} bufferCount={_wristBuffer.Count}");

            _visualizer.InitializeGuide(_exerciseDef, SmoothedWristPos, _armLength);

            _hud?.ShowWarning("Levante o braço até onde for confortável, depois confirme.");
            _hud?.ShowConfirmPeakButton();
        }

        // Step 2 — patient taps confirm at peak
        private void HandlePeakConfirm()
        {
            if (_wrist == null || _wristBuffer.Count == 0) return;

            Vector3 peakPos = SmoothedWristPos;
            if (float.IsNaN(peakPos.x)) return;

            Transform otherShoulder = _pointList.GetChild(_isLeftTarget ? 11 : 12);
            float shoulderWidth = Vector3.Distance(_shoulder.position, otherShoulder.position);

            _evaluator.CalibrateAndBegin(
                _shoulder.position, _elbow.position, peakPos, shoulderWidth, _isLeftTarget);

            _visualizer.PlacePeakMarker(SmoothedWristPos, _armLength);

            _isCalibrated = true;
            _peakConfirmed = true;

            _hud?.HideConfirmPeakButton();
            _hud?.ShowWarning("Agora comece o exercício.");
        }

        protected override void OnEvaluateFrame()
        {
            //          bool left = _def != null && _isLeftTarget;

            // Phase 1: cache landmarks as soon as pointList is available.
            // runs every frame unconditionally: no calibration dependency.
            if (_shoulder == null)
            {
                _shoulder = _pointList.GetChild(_isLeftTarget ? 12 : 11);
                _elbow = _pointList.GetChild(_isLeftTarget ? 14 : 13);
                _wrist = _pointList.GetChild(_isLeftTarget ? 16 : 15);
            }

            // Phase 2: fill the smoothing buffer every frame as long as wrist exists.
            // Also runs before calibration so the buffer is ready when the patient taps.

            if (_wrist != null)
            {
                Vector3 pos = GetFilteredLandmark(_isLeftTarget ? 16 : 15);
                if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
                {
                    _wristBuffer.Enqueue(pos);
                    if (_wristBuffer.Count > BufferSize)
                        _wristBuffer.Dequeue();
                }
            }

            // Phase 3: evaluation only runs after full calibration.
            if (!_isCalibrated || !_peakConfirmed) return;

            Vector3 shoulder = GetFilteredLandmark(_isLeftTarget ? 12 : 11);
            Vector3 elbow = GetFilteredLandmark(_isLeftTarget ? 14 : 13);
            Vector3 wrist = GetFilteredLandmark(_isLeftTarget ? 16 : 15);

            _evaluator.EvaluateFrame(shoulder, elbow, wrist, out float progress);

            _visualizer.UpdateVisuals(_wrist.position, progress);
        }

        protected override void OnSessionComplete()
        {
            Debug.Log("[ElbowFlexion] Session complete.");
        }
        private Vector3 SmoothedWristPos
        {
            get
            {
                Vector3 sum = Vector3.zero;
                foreach (var v in _wristBuffer) sum += v;
                return sum / _wristBuffer.Count;
            }
        }
    }
}
using UnityEngine;
using App.Data.ScriptableObjects;
using App.Vision.Evaluators;
using System.Collections.Generic;

namespace App.Vision.Extractors
{
    public class ElbowFlexionExtractor : BaseExerciseExtractor
    {
        private ElbowFlexionEvaluator _evaluator;
        private Transform _shoulder, _elbow, _wrist;
        private bool _peakConfirmed = false;
        private Queue<Vector3> _wristBuffer = new Queue<Vector3>();
        private const int BufferSize = 8;
        private float _armLength = 1f;

        protected override void OnInitialize()
        {
            _evaluator = new ElbowFlexionEvaluator(_exerciseDef as ElbowFlexionDefinition);
            _evaluator.OnWarningTriggered += HandleBadPosture;
            _evaluator.OnPostureRestored += HandlePostureRestored;
            _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;

            // Wire peak confirm — same pattern as ShoulderSlide
            if (_hud != null)
                _hud.OnPeakConfirmRequested += HandlePeakConfirm;

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_hud != null)
                _hud.OnPeakConfirmRequested -= HandlePeakConfirm;
        }

        // Step 1 — patient taps calibrate with arm at rest
        protected override void CalibrateAndStart()
        {
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

            Debug.Log($"[ElbowFlexion] upperArm={upperArmLength:F1}px " +
                      $"foreArm={foreArmLength:F1}px " +
                      $"using={_armLength:F1}px");

            _visualizer.InitializeGuide(_exerciseDef, restPos, armLength: _armLength);

            _hud?.ShowWarning("Levante o braço até onde for confortável, depois confirme.");
            _hud?.ShowConfirmPeakButton();
        }

        // Step 2 — patient taps confirm at peak
        private void HandlePeakConfirm()
        {
            if (_wrist == null || _wristBuffer.Count == 0) return;

            Vector3 peakPos = SmoothedWristPos;
            if (float.IsNaN(peakPos.x)) return;

            Transform otherShoulder = _pointList.GetChild(
                (_exerciseDef as ElbowFlexionDefinition).isRightArm ? 11 : 12);
            float shoulderWidth = Vector3.Distance(_shoulder.position, otherShoulder.position);

            _evaluator.CalibrateAndBegin(
                _shoulder.position, _elbow.position, peakPos, shoulderWidth);

            _visualizer.PlaceElbowPeakRing(peakPos, _armLength);

            _isCalibrated = true;
            _peakConfirmed = true;

            _hud?.HideConfirmPeakButton();
            _hud?.ShowWarning("Agora comece o exercício.");
        }

        protected override void OnEvaluateFrame()
        {
            // Phase 1: cache landmarks as soon as pointList is available.
            // This runs every frame unconditionally — no calibration dependency.
            if (_shoulder == null)
            {
                var def = _exerciseDef as ElbowFlexionDefinition;
                bool right = def != null && def.isRightArm;
                _shoulder = _pointList.GetChild(right ? 12 : 11);
                _elbow = _pointList.GetChild(right ? 14 : 13);
                _wrist = _pointList.GetChild(right ? 16 : 15);
            }

            // Phase 2: fill the smoothing buffer every frame as long as wrist exists.
            // Also runs before calibration so the buffer is ready when the patient taps.
            /*             if (_wrist != null && Time.frameCount % 60 == 0)
                        {
                            Debug.Log($"[ElbowFlexion] shoulder={_shoulder.position} " +
                                      $"elbow={_elbow.position} " +
                                      $"wrist={_wrist.position}");
                        } */
            if (_wrist != null)
            {
                Vector3 pos = _wrist.position;
                if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
                {
                    _wristBuffer.Enqueue(pos);
                    if (_wristBuffer.Count > BufferSize)
                        _wristBuffer.Dequeue();
                }
            }

            // Phase 3: evaluation only runs after full calibration.
            // if (!_isCalibrated || !_peakConfirmed) return;
            if (!_isCalibrated || !_peakConfirmed)
            {
                if (Time.frameCount % 60 == 0)
                    Debug.Log($"[ElbowFlexion] Blocked: calibrated={_isCalibrated} peakConfirmed={_peakConfirmed}");
                return;
            }

            _evaluator.EvaluateFrame(
                _shoulder.position,
                _elbow.position,
                _wrist.position,
                out float progress);

            _visualizer.UpdateElbowRings(_wrist.position, progress);
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
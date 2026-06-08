using App.Data.ScriptableObjects;
using App.Vision.Evaluators;
using UnityEngine;

public class ShoulderSlideExtractor : BaseExerciseExtractor
{
    private ShoulderSlideEvaluator _evaluator;
    private Transform _shoulder, _elbow, _wrist;

    protected override void OnInitialize()
    {
        _evaluator = new ShoulderSlideEvaluator(_exerciseDef as ShoulderSlideDefinition);
        _evaluator.OnWarningTriggered += HandleBadPosture;
        _evaluator.OnPostureRestored += HandlePostureRestored;
        _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;
        _evaluator.OnCalibrationReady += HandleCalibrationReady;
        _evaluator.OnDiscoveryCompleted += HandleDiscoveryCompleted;

        if (_hud != null)
            _hud.OnPeakConfirmRequested += ConfirmPeak;
    }

    protected override void CalibrateAndStart()
    {
        if (_wrist == null) return;
        _evaluator.CalibrateBaseline(_wrist.position);

        float _armLength = CalculateArmLength();

        _visualizer.InitializeGuide(_exerciseDef, _wrist.position, _armLength);
        _isCalibrated = true;
    }

    protected override void OnEvaluateFrame()
    {
        // Cache landmarks if needed
        if (_shoulder == null)
        {
            var def = _exerciseDef as ShoulderSlideDefinition;
            _shoulder = _pointList.GetChild(def.isRightArm ? 12 : 11);
            _elbow = _pointList.GetChild(def.isRightArm ? 14 : 13);
            _wrist = _pointList.GetChild(def.isRightArm ? 16 : 15);
        }

        if (!_isCalibrated) return;

        _evaluator.EvaluateFrame(
            _shoulder.position,
            _elbow.position,
            _wrist.position,
            out float currentProgress,
            out bool isDiscovering);
        _visualizer.UpdateVisuals(_wrist.position, currentProgress);
    }

    protected override void OnSessionComplete()
    {
        Debug.Log("[ShoulderSlide] Session complete.");
        // navigate to results screen etc.
    }

    // Shoulder-specific handlers
    private void ConfirmPeak()
    {
        _evaluator.ConfirmPeak(_wrist.position);
        Vector3 startPos = new Vector3(_evaluator.StartX, _evaluator.StartY, _wrist.position.z);

        float _armLength = CalculateArmLength();

        _visualizer.InitializeGuide(_exerciseDef, startPos, _armLength); // re-init with correct startPos
        _visualizer.PlacePeakMarker(new Vector3(_evaluator.StartX, _evaluator.MaxY, _wrist.position.z),
                                     _armLength);
    }
    private void HandleCalibrationReady()
    {
        _hud.HideWarning();
        _hud.ShowConfirmPeakButton();
        _hud.ShowWarning("Levante o seu braço até onde for confortável, depois confirme.");
    }
    private void HandleDiscoveryCompleted()
    {
        _hud.HideConfirmPeakButton();
        _hud.HideWarning();
        _hud.ShowWarning("Agora comece o exercicio.");
    }

    private float CalculateArmLength()
    {

        float upperArmLength = Vector3.Distance(_shoulder.position, _elbow.position);
        float foreArmLength = Vector3.Distance(_elbow.position, _wrist.position);

        float armLength = Mathf.Max(upperArmLength, foreArmLength);

        return armLength;
    }
}
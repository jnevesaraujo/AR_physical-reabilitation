using App.Data.ScriptableObjects;
using App.UI;
using App.Vision;
using UnityEngine;

public abstract class BaseExerciseExtractor : MonoBehaviour
{
    // Shared dependencies
    protected ExerciseDefinition _exerciseDef;
    protected ExerciseHUD _hud;
    protected ARExerciseVisualizer _visualizer;

    // Shared state
    protected bool _isCalibrated = false;
    protected int _currentRepetitions = 0;

    // Skeleton cache — every exercise uses Point List Annotation
    protected Transform _pointList;
    protected virtual int RequiredLandmarkCount => 33;

    // Shared initialization wiring
    public void Initialize(ExerciseDefinition definition, ExerciseHUD hud, ARExerciseVisualizer visualizer)
    {
        _exerciseDef = definition;
        _hud = hud;
        _visualizer = visualizer;

        if (_hud != null)
            _hud.OnCalibrationRequested += CalibrateAndStart;

        OnInitialize(); // subclass-specific setup
    }

    protected virtual void DestroyImmediate()
    {
        if (_hud != null)
            _hud.OnCalibrationRequested -= CalibrateAndStart;
    }

    protected void Update()
    {
        if (_pointList == null)
        {
            GameObject found = GameObject.Find("Point List Annotation");
            if (found == null || found.transform.childCount < RequiredLandmarkCount) return;
            _pointList = found.transform;
        }

        if (_pointList != null)
            OnEvaluateFrame();

        if (WorldLandmarkProvider.IsReady)
        {
            Vector3 leftShoulder = WorldLandmarkProvider.GetPosition(11);
            Vector3 rightShoulder = WorldLandmarkProvider.GetPosition(12);
            Debug.Log($"L Shoulder: {leftShoulder} | R Shoulder: {rightShoulder}");
        }
    }

    // Shared event handlers
    protected void HandleBadPosture(string message)
    {
        if (_visualizer != null) _visualizer.SetFeedbackMode(false);
        if (_hud != null) _hud.ShowWarning(message);
    }

    protected void HandlePostureRestored()
    {
        if (_visualizer != null) _visualizer.SetFeedbackMode(true);
        if (_hud != null) _hud.HideWarning();
    }

    protected void HandleRepetitionSuccess()
    {
        _currentRepetitions++;
        if (_hud != null)
            _hud.UpdateRepetitionCount(_currentRepetitions, _exerciseDef.targetRepetitions);
        if (_visualizer != null)
            _visualizer.TriggerSuccessFeedback();

        if (_currentRepetitions >= _exerciseDef.targetRepetitions)
            OnSessionComplete();
    }

    // Abstract — subclasses must implement these
    protected abstract void OnInitialize();       // wire up evaluator events
    protected abstract void CalibrateAndStart();  // exercise-specific calibration
    protected abstract void OnEvaluateFrame();    // call evaluator with correct landmarks
    protected abstract void OnSessionComplete();  // handle end of session
}
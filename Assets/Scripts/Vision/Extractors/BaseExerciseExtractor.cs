using System.Collections.Generic;
using App.Core;
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

    [Header("Landmark Smoothing")]
    [Range(0.1f, 3.0f)]
    [Tooltip("Lower = smoother when still, more lag. Start at 1.0.")]
    [SerializeField] private float _filterMinCutoff = 1.0f;

    [Range(0.0f, 2.0f)]
    [Tooltip("Higher = less lag during fast movement. Start at 0.1.")]
    [SerializeField] private float _filterBeta = 0.1f;

    private Dictionary<int, OneEuroFilterV3> _landmarkFilters = new Dictionary<int, OneEuroFilterV3>();

    protected Vector3 GetFilteredLandmark(int childIndex)
    {
        if (_pointList == null) return Vector3.zero;

        var raw = _pointList.GetChild(childIndex).position;

        if (!_landmarkFilters.TryGetValue(childIndex, out var filter))
        {
            filter = new OneEuroFilterV3(_filterMinCutoff, _filterBeta);
            _landmarkFilters[childIndex] = filter;
        }

        return filter.Filter(raw, Time.time);
    }

    protected void ResetLandmarkFilters()
    {
        foreach (var f in _landmarkFilters.Values)
            f.Reset();
    }
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

    protected virtual void OnDestroy()
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
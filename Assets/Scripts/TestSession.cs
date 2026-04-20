using UnityEngine;
using UnityEngine.InputSystem;
using App.Data.ScriptableObjects;
using App.Vision;
using App.UI;

public class TestSession : MonoBehaviour
{
    [Header("Data & Configuration")]
    public NeckRotationDefinition exerciseDefinition;

    [Header("Architecture References")]
    public ARExerciseVisualizer visualizer;
    public ExerciseHUD exerciseHUD;
    private NeckRotationEvaluator _evaluator;
    private bool _isCalibrated = false;
    private Transform _nose, _leftShoulder, _rightShoulder;
    private int _currentRepetitions = 0;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    void Start()
    {
        _evaluator = new NeckRotationEvaluator(exerciseDefinition);

        if (exerciseHUD != null)
            exerciseHUD.InitializeHUD(exerciseDefinition.targetRepetitions);

        _evaluator.OnWarningTriggered += HandleBadPosture;
        _evaluator.OnPostureRestored += HandlePostureRestored;
        _evaluator.OnMovementTracked += HandleMovement;
        _evaluator.OnRepetitionCompleted += HandleRepetitionSuccess;
    }

    void Update()
    {
        if (_nose == null)
        {
            GameObject pointList = GameObject.Find("Point List Annotation");
            if (pointList == null || pointList.transform.childCount < 33) return;

            _nose = pointList.transform.GetChild(0);
            _leftShoulder = pointList.transform.GetChild(11);
            _rightShoulder = pointList.transform.GetChild(12);
        }

        /*         if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    float currentShoulderDiff = AngleCalculator.GetVerticalDifference(
                        _leftShoulder.position, _rightShoulder.position);

                    _evaluator.CalibrateOrigin(_nose.position, currentShoulderDiff);
                    _isCalibrated = true;

                    visualizer.InitializeGuide(exerciseDefinition, _nose);
                    Debug.Log("<color=green>Calibration Complete!</color>");
                } */

        if (_isCalibrated)
        {
            _evaluator.EvaluateFrame(_nose, _leftShoulder, _rightShoulder);
        }
    }

    public void CalibrateAndStart()
    {
        if (_nose == null || _leftShoulder == null)
        {
            Debug.LogWarning("Cannot calibrate: MediaPipe skeleton not found yet.");
            return;
        }

        float currentShoulderDiff = AngleCalculator.GetVerticalDifference(
            _leftShoulder.position, _rightShoulder.position);

        _evaluator.CalibrateOrigin(_nose.position, currentShoulderDiff);
        _isCalibrated = true;

        visualizer.InitializeGuide(exerciseDefinition, _nose.position);

        if (exerciseHUD != null) exerciseHUD.HideWarning();

        Debug.Log("<color=green>Mobile Calibration Complete!</color>");
    }

    // --- Event Handlers ---

    private void HandleBadPosture(string warningMessage)
    {
        visualizer.SetFeedbackMode(false);
        if (exerciseHUD != null) exerciseHUD.ShowWarning(warningMessage);
    }

    private void HandlePostureRestored()
    {
        visualizer.SetFeedbackMode(true);
        if (exerciseHUD != null) exerciseHUD.HideWarning();
    }

    private void HandleMovement(float currentAngle)
    {
        /*         Debug.Log($"Current Angle: {currentAngle:F1}"); */
    }

    private void HandleRepetitionSuccess()
    {
        _currentRepetitions++;

        if (exerciseHUD != null)
        {
            exerciseHUD.UpdateRepetitionCount(_currentRepetitions, exerciseDefinition.targetRepetitions);
        }

        Debug.Log("<color=yellow>Repetition Completed!</color>");

        if (_currentRepetitions >= exerciseDefinition.targetRepetitions)
        {
            Debug.Log("<color=cyan>Session Complete!</color>");
        }
    }

    void OnDestroy()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
}
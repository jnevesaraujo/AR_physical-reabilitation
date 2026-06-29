using System;
using App.UI.Toolkit;
using App.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


namespace App.UI
{
    public class ExerciseHUD : MonoBehaviour
    {
        public event Action OnPeakConfirmRequested, OnCalibrationRequested;
        public ExerciseTimerController _timerController;
        // UI Toolkit
        private Label _lblRepetitions, _lblFeedback, _lblTimer;
        private VisualElement _btnConfirm, _btnCalibrate, _btnBack, _visualFeedback;
        public static bool isReturningFromSession;

        private void OnEnable()
        {
            isReturningFromSession = false;
            var root = GetComponent<UIDocument>().rootVisualElement;

            _visualFeedback = root.Q<VisualElement>("visual_feedback");

            _lblRepetitions = root.Q<Label>("label_reps");
            _lblFeedback = root.Q<Label>("label_feedback");
            _lblTimer = root.Q<Label>("label_timer");
            _btnConfirm = root.Q<VisualElement>("btn_confirm");
            _btnCalibrate = root.Q<VisualElement>("btn_calibrate");
            _btnBack = root.Q<VisualElement>("btn_back");

            _btnConfirm.RegisterCallback<ClickEvent>(OnBtnConfirmClick);
            _btnCalibrate.RegisterCallback<ClickEvent>(OnBtnCalibrateClick);
            _btnBack.RegisterCallback<ClickEvent>(_ => ReturnToMainMenu());

            VisualElement[] btns = { _btnConfirm, _btnCalibrate, _btnBack };
            foreach (var btn in btns)
                btn?.AddTouchFeedback();
        }

        private void OnDisable()
        {
            // Unregister callbacks to prevent potential memory leaks
            _btnConfirm.UnregisterCallback<ClickEvent>(OnBtnConfirmClick);
            _btnCalibrate.UnregisterCallback<ClickEvent>(OnBtnCalibrateClick);
            _btnBack.UnregisterCallback<ClickEvent>(_ => ReturnToMainMenu());
        }
        public void InitializeHUD(int targetRepetitions)
        {
            UpdateRepetitionCount(0, targetRepetitions);
            HideWarning();
            HideConfirmPeakButton();
            Debug.Log($"[HUD] HUD inicializado com target de {targetRepetitions} repetições.");
        }

        public void UpdateRepetitionCount(int currentReps, int targetReps)
        {
            SessionContext.CurrentRepetitions = currentReps;
            _lblRepetitions.text = $"{currentReps} / {targetReps}";

            if (currentReps >= targetReps && targetReps > 0)
                ReturnToMainMenu();
        }

        public void ShowWarning(string message)
        {
            if (_visualFeedback != null)
            {
                _lblFeedback.text = message;
                _visualFeedback.style.color = new StyleColor(Color.red);
                _visualFeedback.style.display = DisplayStyle.Flex;
            }
        }

        public void HideWarning()
        {
            if (_visualFeedback != null)
            {
                _visualFeedback.style.display = DisplayStyle.None;
            }
        }

        public void TriggerCalibration()
        {
            Debug.Log("[HUD] Botão de calibração pressionado!");
            OnCalibrationRequested?.Invoke();
            _timerController = GetComponent<ExerciseTimerController>();
            _timerController?.StartTimer();
        }

        public void ShowConfirmPeakButton()
        {
            if (_btnConfirm != null)
            {
                _btnConfirm.style.display = DisplayStyle.Flex;
            }
        }

        public void HideConfirmPeakButton()
        {
            if (_btnConfirm != null)
            {
                _btnConfirm.style.display = DisplayStyle.None;
            }
        }

        private void OnConfirmPeakButtonPressed()
        {
            OnPeakConfirmRequested?.Invoke();
            HideConfirmPeakButton();
        }

        public void ReturnToMainMenu()
        {
            _timerController?.PauseTimer();
            SessionContext.ElapsedSeconds = _timerController.ElapsedSeconds;
            SessionContext.ReturnToExerciseMenu = true;
            isReturningFromSession = true;
            SceneManager.LoadScene("App_Menu");
        }

        private void OnBtnConfirmClick(ClickEvent evt) => OnConfirmPeakButtonPressed();
        private void OnBtnCalibrateClick(ClickEvent evt) => TriggerCalibration();
    }
}
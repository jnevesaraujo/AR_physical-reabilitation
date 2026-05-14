/* using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

namespace App.UI
{
    public class ExerciseHUD : MonoBehaviour
    {
        public event Action OnPeakConfirmRequested;
        public Button Confirm_btn;
        [Header("Text References")]
        public TextMeshProUGUI repetitionText;
        public TextMeshProUGUI warningText;
        public event Action OnCalibrationRequested;

        public void InitializeHUD(int targetRepetitions)
        {
            UpdateRepetitionCount(0, targetRepetitions);
            HideWarning();
        }

        public void UpdateRepetitionCount(int currentReps, int targetReps)
        {
            repetitionText.text = $"Reps: {currentReps} / {targetReps}";
        }

        public void ShowWarning(string message)
        {
            warningText.text = message;
            warningText.color = Color.red;
            warningText.gameObject.SetActive(true);
        }

        public void HideWarning()
        {
            warningText.gameObject.SetActive(false);
        }

        public void TriggerCalibration()
        {
            Debug.Log("[HUD] Botão de calibração pressionado!");
            OnCalibrationRequested?.Invoke();
        }

        public void ShowConfirmPeakButton()
        {
            Confirm_btn.gameObject.SetActive(true);
        }

        public void HideConfirmPeakButton()
        {
            Confirm_btn.gameObject.SetActive(false);
        }
        public void OnConfirmPeakButtonPressed()
        {
            OnPeakConfirmRequested?.Invoke();
            HideConfirmPeakButton();
        }
    }
} */
using System;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


namespace App.UI
{
    public class ExerciseHUD : MonoBehaviour
    {
        public event Action OnPeakConfirmRequested, OnCalibrationRequested;
        // UI Toolkit
        private Label _lblRepetitions, _lblFeedback;
        private VisualElement _btnConfirm, _btnCalibrate, _btnBack, _visualFeedback;
        public static bool isReturningFromSession;

        private void OnEnable()
        {
            isReturningFromSession = false;
            var root = GetComponent<UIDocument>().rootVisualElement;

            _visualFeedback = root.Q<VisualElement>("visual_feedback");
            _lblRepetitions = root.Q<Label>("label_reps");
            _lblFeedback = root.Q<Label>("label_feedback");

            _btnConfirm = root.Q<VisualElement>("btn_confirm");
            _btnCalibrate = root.Q<VisualElement>("btn_calibrate");
            _btnBack = root.Q<VisualElement>("btn_back");

            _btnConfirm.RegisterCallback<ClickEvent>(OnBtnConfirmClick);
            _btnCalibrate.RegisterCallback<ClickEvent>(OnBtnCalibrateClick);
            _btnBack.RegisterCallback<ClickEvent>(_ => ReturnToMainMenu());
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
            if (_lblRepetitions != null)
            {
                _lblRepetitions.text = $"{currentReps} / {targetReps}";
            }
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
            SessionContext.ReturnToExerciseMenu = true;
            isReturningFromSession = true;
            SceneManager.LoadScene("App_Menu");
        }

        private void OnBtnConfirmClick(ClickEvent evt) => OnConfirmPeakButtonPressed();
        private void OnBtnCalibrateClick(ClickEvent evt) => TriggerCalibration();
    }
}
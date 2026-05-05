using UnityEngine;
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
}
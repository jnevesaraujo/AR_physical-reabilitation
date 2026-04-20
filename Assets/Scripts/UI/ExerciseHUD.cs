using UnityEngine;
using TMPro; // Required for TextMeshPro

namespace App.UI
{
    public class ExerciseHUD : MonoBehaviour
    {
        [Header("Text References")]
        public TextMeshProUGUI repetitionText;
        public TextMeshProUGUI warningText;

        public void InitializeHUD(int targetRepetitions)
        {
            UpdateRepetitionCount(0, targetRepetitions);
            HideWarning();
        }

        public void UpdateRepetitionCount(int currentReps, int targetReps)
        {
            if (repetitionText != null)
            {
                repetitionText.text = $"Reps: {currentReps} / {targetReps}";
            }
        }

        public void ShowWarning(string message)
        {
            if (warningText != null)
            {
                warningText.text = message;
                warningText.color = Color.red;
                warningText.gameObject.SetActive(true);
            }
        }

        public void HideWarning()
        {
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using App.Core;
using App.UI.Views;
using App.UI.Toolkit;
using System;

namespace App.Controllers
{
    [RequireComponent(typeof(ExerciseHUDView))]
    public class ExerciseHUDController : MonoBehaviour
    {
        [SerializeField] private ExerciseTimerController timerController;
        private ExerciseHUDView _view;
        private int _targetRepetitions;
        public event Action OnPeakConfirmed;
        public event Action OnCalibrationStarted;

        private void Awake()
        {
            _view = GetComponent<ExerciseHUDView>();
            
            _view.OnConfirmRequested += HandleConfirmRequested;
            _view.OnCalibrateRequested += HandleCalibrateRequested;
            _view.OnBackRequested += ReturnToMainMenu;
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnConfirmRequested -= HandleConfirmRequested;
                _view.OnCalibrateRequested -= HandleCalibrateRequested;
                _view.OnBackRequested -= ReturnToMainMenu;
            }
        }

        public void InitializeHUD(int targetReps)
        {
            _targetRepetitions = targetReps;
            UpdateRepetitionCount(0);
            _view.HideWarning();
            _view.SetConfirmButtonVisibility(false);
        }

        public void UpdateRepetitionCount(int currentReps)
        {
            SessionContext.CurrentRepetitions = currentReps;
            _view.UpdateRepetitionsText($"{currentReps} / {_targetRepetitions}");

            if (currentReps >= _targetRepetitions && _targetRepetitions > 0)
            {
                ReturnToMainMenu();
            }
        }

        private void HandleConfirmRequested()
        {
            OnPeakConfirmed?.Invoke();
            _view.SetConfirmButtonVisibility(false);
        }

        private void HandleCalibrateRequested()
        {
            OnCalibrationStarted?.Invoke();
            if (timerController != null) timerController.StartTimer();
        }

        public void ReturnToMainMenu()
        {
            if (timerController != null)
            {
                timerController.PauseTimer();
                SessionContext.ElapsedSeconds = timerController.ElapsedSeconds;
            }
            
            SessionContext.ReturnToExerciseMenu = true;
            SceneManager.LoadScene("App_Menu");
        }
        public void ShowWarning(string message)
        {
            if (_view != null) _view.ShowWarning(message);
        }

        public void HideWarning()
        {
            if (_view != null) _view.HideWarning();
        }

        public void ShowConfirmPeakButton()
        {
            if (_view != null) _view.SetConfirmButtonVisibility(true);
        }

        public void HideConfirmPeakButton()
        {
            if (_view != null) _view.SetConfirmButtonVisibility(false);
        }
    }
}
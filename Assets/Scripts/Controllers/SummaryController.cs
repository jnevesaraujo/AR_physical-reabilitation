using System;
using UnityEngine;
using App.Core;
using App.Data.Models;
using App.Services;
using App.UI.Toolkit;

namespace App.Controllers
{
    [RequireComponent(typeof(SummaryView), typeof(UINavigationManager))]
    public class SummaryController : MonoBehaviour
    {
        private SummaryView _view;
        private UINavigationManager _navManager;

        private void Awake()
        {
            _view = GetComponent<SummaryView>();
            _navManager = GetComponent<UINavigationManager>();

            _view.OnCloseRequested += HandleCloseRequested;
        }

        private void OnEnable()
        {
            PopulateSummaryData();
            AutoSubmitSessionAsync();
        }

        private void PopulateSummaryData()
        {
            if (SessionContext.CurrentExercise == null)
            {
                _view.SetExerciseName(string.Empty);
                _view.SetResults(string.Empty, string.Empty);
                return;
            }

            _view.SetExerciseName(SessionContext.CurrentExercise.exerciseName);

            string timeString = SessionContext.ElapsedSeconds > 0 
                ? $"{SessionContext.ElapsedSeconds / 60} minutos e {SessionContext.ElapsedSeconds % 60} segundos" 
                : "N/A";

            _view.SetResults(timeString, SessionContext.CurrentRepetitions.ToString());
        }

        private async void AutoSubmitSessionAsync()
        {
            if (SessionContext.CurrentExercise == null || SessionContext.UserId == null)
            {
                _view.SetStatusMessage(string.Empty);
                return;
            }

            _view.SetStatusMessage("A sincronizar dados...");

            try
            {
                var record = new SessionRecord
                {
                    exerciseId = SessionContext.CurrentExercise?.name ?? "unknown",
                    sessionTimestamp = DateTime.UtcNow,
                    completedReps = SessionContext.CurrentRepetitions,
                    targetReps = SessionContext.CurrentExercise?.targetRepetitions ?? 0,
                    accuracyScore = 0f,
                    durationSeconds = SessionContext.ElapsedSeconds,
                    isCompleted = SessionContext.CurrentRepetitions >= (SessionContext.CurrentExercise?.targetRepetitions ?? 1)
                };

                var sessionService = new SessionService();
                sessionService.Initialize(SessionContext.UserId);

                await sessionService.SaveSessionAsync(record);
                
                var profileService = new ProfileService();
                await profileService.IncrementSessionCountAsync(SessionContext.UserId);

                if (SessionContext.CurrentUser != null)
                    SessionContext.CurrentUser.totalSessionsCompleted++;

                _view.SetStatusMessage("Dados guardados com sucesso.");
                Debug.Log("[Firestore] Registo submetido via Auto-Save.");
            }
            catch (Exception ex)
            {
                _view.SetStatusMessage("Erro ao guardar dados.");
                Debug.LogError($"[Firestore] Falha na auto-submissão: {ex.Message}");
            }
        }

        private void HandleCloseRequested()
        {
            SessionContext.ClearExerciseSession();
            SessionContext.ReturnToExerciseMenu = false;
            _navManager?.NavigateTo(AppScreen.Exercises);
        }

        private void OnDestroy()
        {
            if (_view != null) _view.OnCloseRequested -= HandleCloseRequested;
        }
    }
}
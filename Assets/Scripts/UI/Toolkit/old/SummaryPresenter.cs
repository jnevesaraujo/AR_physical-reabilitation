using UnityEngine;
using UnityEngine.UIElements;
using App.Core;
using App.Data.Models;
using System;
using App.Services;
using System.Threading.Tasks;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class SummaryPresenter : MonoBehaviour
    {
        private VisualElement _root;
        private Label _lblExerciseName, _lblTimeResult, _lblRepResult, _lblStatusInfo;
        private Button _btnCloseSummary;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _lblExerciseName = _root.Q<Label>("summary_exercise_name");
            _lblTimeResult = _root.Q<Label>("summary_time_result");
            _lblRepResult = _root.Q<Label>("summary_rep_result");
            _lblStatusInfo = _root.Q<Label>("summary_status_info");
            _btnCloseSummary = _root.Q<Button>("btn_close_summary");
            _btnCloseSummary.RegisterCallback<ClickEvent>(_ => CloseSummary());

            PopulateSummaryData();

            AutoSubmitSessionAsync();
        }

        private void PopulateSummaryData()
        {
            if (SessionContext.CurrentExercise == null)
            {
                // No session data yet — summary will populate when navigated to
                if (_lblExerciseName != null) _lblExerciseName.text = string.Empty;
                if (_lblTimeResult != null) _lblTimeResult.text = string.Empty;
                if (_lblRepResult != null) _lblRepResult.text = string.Empty;
                return;
            }

            if (_lblExerciseName != null)
                _lblExerciseName.text = SessionContext.CurrentExercise.exerciseName;

            if (_lblTimeResult != null)
                _lblTimeResult.text = SessionContext.ElapsedSeconds > 0 ? $"{SessionContext.ElapsedSeconds / 60} minutos e {SessionContext.ElapsedSeconds % 60} segundos" : "N/A";

            if (_lblRepResult != null)
                _lblRepResult.text = SessionContext.CurrentRepetitions.ToString();
        }

        private async void AutoSubmitSessionAsync()
        {
            if (SessionContext.CurrentExercise == null || SessionContext.UserId == null)
            {
                if (_lblStatusInfo != null) _lblStatusInfo.text = string.Empty;
                return;
            }

            if (_lblStatusInfo != null) _lblStatusInfo.text = "A sincronizar dados...";

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

                // Update cached value so home screen reflects the new count immediately
                if (SessionContext.CurrentUser != null)
                    SessionContext.CurrentUser.totalSessionsCompleted++;

                if (_lblStatusInfo != null) _lblStatusInfo.text = "Dados guardados com sucesso.";
                Debug.Log("[Firestore] Registo submetido via Auto-Save.");
            }
            catch (Exception ex)
            {
                if (_lblStatusInfo != null) _lblStatusInfo.text = "Erro ao guardar dados.";
                Debug.LogError($"[Firestore] Falha na auto-submissão: {ex.Message}");
            }
        }

        public void CloseSummary()
        {
            SessionContext.ClearExerciseSession();
            var navManager = GetComponent<UINavigationManager>();
            navManager?.NavigateTo(AppScreen.Exercises);
            SessionContext.ReturnToExerciseMenu = false;
        }
    }
}
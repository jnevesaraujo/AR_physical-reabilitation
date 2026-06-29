using UnityEngine;
using UnityEngine.UIElements;
using App.Core;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class HomePresenter : MonoBehaviour
    {
        private VisualElement _root;
        private UINavigationManager _navManager;
        // Labels
        private Label _lblUsername;
        private Label _lblSubjectId;
        private Label _lblSessionsCompleted;
        private Label _lblAffectedSide;

        // Progress bar
        private ProgressBar _progressBar;

        // Navigation button
        private VisualElement _btnGoExercises;

        // Total sessions expected per test day — 4 exercises, 1 session each
        private const int TotalDailySessions = 4;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _lblUsername         = _root.Q<Label>("home_lbl_username");
            _lblSubjectId        = _root.Q<Label>("subject_id_home_label");
            _lblSessionsCompleted = _root.Q<Label>("session_completed");
            _lblAffectedSide     = _root.Q<Label>("lbl_affected-side");
            _progressBar         = _root.Q<ProgressBar>("SessionProgressBar");
            _btnGoExercises      = _root.Q<VisualElement>("go_exercises_btn");

            _btnGoExercises?.AddTouchFeedback();
            _btnGoExercises?.RegisterCallback<ClickEvent>(_ => _navManager.NavigateTo(AppScreen.Exercises));

            _navManager = GetComponent<UINavigationManager>();
            if (_navManager != null)
                _navManager.OnNavigatedTo += HandleNavigation;
        }

        private void OnDisable()
        {
            if (_navManager != null)
                _navManager.OnNavigatedTo -= HandleNavigation;
        }

        private void HandleNavigation(AppScreen screen)
        {
            if (screen == AppScreen.Home)
                PopulateHomeData();
        }

        private void PopulateHomeData()
        {
            if (SessionContext.CurrentUser == null)
            {
                Debug.LogWarning("[HomePresenter] CurrentUser is null during home population");
                return;
            }

            var user = SessionContext.CurrentUser;

            // Greeting with first name
            if (_lblUsername != null)
                _lblUsername.text = $"Olá {user.firstName}!";

            // Subject ID — shown to researcher to confirm active participant
            if (_lblSubjectId != null)
            {
                string subjectDisplay = string.IsNullOrEmpty(user.subjectId)
                    ? "Não definido"
                    : user.subjectId;
                _lblSubjectId.text = $"ID: {subjectDisplay}";
            }

            // Sessions completed today vs total expected
            // totalSessionsCompleted from Firestore is lifetime total.
            // For a daily view you'd need a date-filtered query — for now
            // show completed vs target as a simple progress indicator.
            int completed = user.totalSessionsCompleted;
            int remaining = Mathf.Max(0, TotalDailySessions - completed);

            if (_lblSessionsCompleted != null)
            {
                if (remaining == 0)
                    _lblSessionsCompleted.text = $"{completed}/{TotalDailySessions} — Completo!";
                else
                    _lblSessionsCompleted.text =
                        $"{completed}/{TotalDailySessions} ({remaining} em falta)";
            }

            // Progress bar — value is 0 to 100
            if (_progressBar != null)
            {
                float progressPercent = TotalDailySessions > 0
                    ? (completed / (float)TotalDailySessions) * 100f
                    : 0f;
                _progressBar.value = Mathf.Clamp(progressPercent, 0f, 100f);

                // UI Toolkit ProgressBar title is displayed inside the bar
                _progressBar.title = $"{completed} de {TotalDailySessions} sessões";
            }

            // Affected side
            if (_lblAffectedSide != null)
            {
                _lblAffectedSide.text = string.IsNullOrEmpty(user.affectedSide)
                    ? "Não definido"
                    : user.affectedSide;
            }
        }
    }
}
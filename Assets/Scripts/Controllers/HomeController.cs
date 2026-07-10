using UnityEngine;
using App.Core;
using App.Data.Models;
using App.UI.Views;
using App.UI.Toolkit;

namespace App.Controllers
{
    [RequireComponent(typeof(HomeView), typeof(UINavigationManager))]
    public class HomeController : MonoBehaviour
    {
        private HomeView _view;
        private UINavigationManager _navManager;
        private const int TotalDailySessions = 4;

        private void Awake()
        {
            _view = GetComponent<HomeView>();
            _navManager = GetComponent<UINavigationManager>();
            _view.OnGoExercisesRequested += HandleGoExercises;
        }

        private void OnEnable() 
        { 
            if (_navManager != null) _navManager.OnNavigatedTo += HandleNavigation; 
        }
        
        private void OnDisable() 
        { 
            if (_navManager != null) _navManager.OnNavigatedTo -= HandleNavigation; 
        }

        private void OnDestroy() { if (_view != null) _view.OnGoExercisesRequested -= HandleGoExercises; }

        private void HandleGoExercises() => _navManager.NavigateTo(AppScreen.Exercises);

        private void HandleNavigation(AppScreen screen)
        {
            if (screen == AppScreen.Home) PopulateHomeData();
        }

        private void PopulateHomeData()
        {
            var user = SessionContext.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("[HomeController] CurrentUser is null during home population");
                return;
            }

            _view.SetUsername($"Olá {user.firstName}!");

            string subjectDisplay = string.IsNullOrEmpty(user.subjectId) ? "Não definido" : user.subjectId;
            _view.SetSubjectId($"ID: {subjectDisplay}");

            int completed = user.totalSessionsCompleted;
            int remaining = Mathf.Max(0, TotalDailySessions - completed);
            _view.SetSessionsCompletedText(remaining == 0
                ? $"{completed}/{TotalDailySessions} — Completo!"
                : $"{completed}/{TotalDailySessions} ({remaining} em falta)");

            float progressPercent = TotalDailySessions > 0 ? (completed / (float)TotalDailySessions) * 100f : 0f;
            _view.SetProgress(progressPercent, $"{completed} de {TotalDailySessions} sessões");

            _view.SetAffectedSideText(user.affectedSide switch
            {
                AffectedSide.Left => "Esquerdo",
                AffectedSide.Right => "Direito",
                AffectedSide.Bilateral => "Bilateral",
                _ => "Não definido"
            });
        }
    }
}
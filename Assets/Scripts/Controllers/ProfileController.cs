using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;
using App.Services;
using App.Core;
using UserProfile = App.Data.Models.UserProfile;
using App.UI.Views;
using App.UI.Toolkit;

namespace App.UI.Controllers
{
    [RequireComponent(typeof(ProfileView), typeof(UINavigationManager))]
    public class ProfileController : MonoBehaviour
    {
        private ProfileView _view;
        private UINavigationManager _navManager;
        private ProfileService _profileService;

        private void Awake()
        {
            _view = GetComponent<ProfileView>();
            _navManager = GetComponent<UINavigationManager>();
            _profileService = new ProfileService();

            _view.OnSaveRequested += HandleSaveRequested;
            _view.OnChangePasswordRequested += HandleChangePasswordRequested;
        }

        private void OnEnable() { if (_navManager != null) _navManager.OnNavigatedTo += HandleNavigation; }
        private void OnDisable() { if (_navManager != null) _navManager.OnNavigatedTo -= HandleNavigation; }
        private void OnDestroy()
        {
            if (_view == null) return;
            _view.OnSaveRequested -= HandleSaveRequested;
            _view.OnChangePasswordRequested -= HandleChangePasswordRequested;
        }

        private void HandleNavigation(AppScreen screen)
        {
            if (screen == AppScreen.Profile) PopulateFromSession();
        }

        private void PopulateFromSession()
        {
            var user = SessionContext.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("[ProfileController] CurrentUser is null");
                return;
            }
            _view.SetUsername($"{user.firstName} {user.lastName}");
            _view.SetEmail(user.email);
            _view.SetSubjectId(user.subjectId ?? "");
            _view.SetAffectedSide(user.affectedSide);
            if (!string.IsNullOrEmpty(user.surgeryDate)) _view.SetSurgeryDate(user.surgeryDate);
        }

        private async void HandleSaveRequested()
        {
            if (SessionContext.CurrentUser == null) return;
            _view.SetSaveButtonState(false, "A guardar...");

            var profile = new UserProfile
            {
                userId = SessionContext.CurrentUser.userId,
                subjectId = _view.SubjectIdInput,
                firstName = SessionContext.CurrentUser.firstName,
                lastName = SessionContext.CurrentUser.lastName,
                email = SessionContext.CurrentUser.email,
                registrationDate = SessionContext.CurrentUser.registrationDate,
                totalSessionsCompleted = SessionContext.CurrentUser.totalSessionsCompleted,
                affectedSide = _view.AffectedSideInput,
                surgeryDate = _view.SurgeryDateInput
            };

            try
            {
                await _profileService.CreateProfileAsync(profile);
                SessionContext.CurrentUser.subjectId = profile.subjectId;
                SessionContext.CurrentUser.affectedSide = profile.affectedSide;
                SessionContext.CurrentUser.surgeryDate = profile.surgeryDate;
                _view.SetSaveButtonState(false, "Guardado!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProfileController] Save failed: {e.Message}");
                _view.SetSaveButtonState(false, "Erro.");
            }

            await Task.Delay(2000);
            _view.SetSaveButtonState(true, "Guardar Alterações");
        }

        private void HandleChangePasswordRequested()
        {
            string email = _view.EmailInput;
            if (string.IsNullOrEmpty(email)) return;

            FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(email).ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                    Debug.Log("[ProfileController] Email de redefinição enviado com sucesso!");
            });
        }
    }
}
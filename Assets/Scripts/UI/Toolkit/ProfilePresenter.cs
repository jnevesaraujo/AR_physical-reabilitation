using UnityEngine;
using UnityEngine.UIElements;
using Firebase.Auth;
using App.Services;
using App.Data.Models;
using UserProfile = App.Data.Models.UserProfile;
using System;
using App.Core;

namespace App.UI.Toolkit
{
    public class ProfilePresenter : MonoBehaviour
    {
        private VisualElement _root;
        private Label _profile_lbl_username;
        private TextField _txtSubjectId, _txtEmail, _txtSurgeryDate;
        private DropdownField _drpAffectedSide;
        private Button _btnSave, _btnChangePass;
        private ProfileService _profileService;
        private UINavigationManager _navManager;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[ProfilePresenter] UIDocument não encontrado ou não inicializado!");
                return;
            }
            _root = GetComponent<UIDocument>().rootVisualElement;
            _profileService = new ProfileService();
            _profile_lbl_username = _root.Q<Label>("profile_lbl_username");
            _txtSubjectId = _root.Q<TextField>("profile_subject_id");
            _txtEmail = _root.Q<TextField>("profile_email");
            _txtSurgeryDate = _root.Q<TextField>("profile_surgery_date");
            _drpAffectedSide = _root.Q<DropdownField>("profile_affected_side");

            _btnSave = _root.Q<Button>("btn_save_profile");
            _btnChangePass = _root.Q<Button>("btn_change_password");

            _navManager = GetComponent<UINavigationManager>();
            if (_navManager != null)
                _navManager.OnNavigatedTo += HandleNavigation;

            _btnSave?.RegisterCallback<ClickEvent>(OnSaveClicked);
            _btnChangePass?.RegisterCallback<ClickEvent>(_ => HandleChangePassword());
        }

        private void OnDisable()
        {
            // clean subscriptions to avoid memory leaks
            _btnSave?.UnregisterCallback<ClickEvent>(OnSaveClicked);
            if (_btnChangePass != null) _btnChangePass.clicked -= HandleChangePassword;
            if (_navManager != null)
                _navManager.OnNavigatedTo -= HandleNavigation;
        }

        private async void HandleSaveAsync()
        {
            if (SessionContext.CurrentUser == null) return;

            _btnSave.SetEnabled(false);
            _btnSave.text = "A guardar...";

            // Read subject ID from the field — this is the only field the researcher changes
            string newSubjectId = _txtSubjectId?.value ?? SessionContext.CurrentUser.subjectId;

            var profile = new UserProfile
            {
                userId = SessionContext.CurrentUser.userId,
                subjectId = newSubjectId,
                firstName = SessionContext.CurrentUser.firstName,
                lastName = SessionContext.CurrentUser.lastName,
                email = SessionContext.CurrentUser.email,
                registrationDate = SessionContext.CurrentUser.registrationDate,
                totalSessionsCompleted = SessionContext.CurrentUser.totalSessionsCompleted,
                affectedSide = _drpAffectedSide != null ? MapDropdownToEnum(_drpAffectedSide.value) : SessionContext.CurrentUser.affectedSide,
                surgeryDate = _txtSurgeryDate?.value ?? SessionContext.CurrentUser.surgeryDate
            };

            try
            {
                await _profileService.CreateProfileAsync(profile);

                // Update the cached session user so subsequent sessions use the new subject ID
                SessionContext.CurrentUser.subjectId = profile.subjectId;
                SessionContext.CurrentUser.affectedSide = profile.affectedSide;
                SessionContext.CurrentUser.surgeryDate = profile.surgeryDate;

                _btnSave.text = "Guardado!";
                Debug.Log($"[Profile] Saved. SubjectId={profile.subjectId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Profile] Save failed: {e.Message}");
                _btnSave.text = "Erro.";
            }

            await System.Threading.Tasks.Task.Delay(2000);
            _btnSave.text = "Guardar Alterações";
            _btnSave.SetEnabled(true);
        }

        private void HandleChangePassword()
        {
            if (_txtEmail == null || string.IsNullOrEmpty(_txtEmail.value)) return;

            FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(_txtEmail.value).ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log("[ProfilePresenter] Email de redefinição enviado com sucesso!");
                }
            });
        }
        private void HandleNavigation(AppScreen screen)
        {
            if (screen == AppScreen.Profile)
                PopulateFromSession();
        }

        private void PopulateFromSession()
        {
            if (SessionContext.CurrentUser == null)
            {
                Debug.LogWarning("[ProfilePresenter] PopulateFromSession: CurrentUser is null");
                return;
            }

            Debug.Log($"[ProfilePresenter] Populating — " +
                      $"email={SessionContext.CurrentUser.email} " +
                      $"subjectId={SessionContext.CurrentUser.subjectId}");

            if (_profile_lbl_username != null)
                _profile_lbl_username.text = $"{SessionContext.CurrentUser.firstName} {SessionContext.CurrentUser.lastName}";

            if (_txtEmail != null)
                _txtEmail.value = SessionContext.CurrentUser.email;
            else
                Debug.LogWarning("[ProfilePresenter] profile_email field not found");

            if (_txtSubjectId != null)
                _txtSubjectId.value = SessionContext.CurrentUser.subjectId ?? "";
            else
                Debug.LogWarning("[ProfilePresenter] profile_subject_id field not found");

            if (_drpAffectedSide != null)
                _drpAffectedSide.value = MapEnumToDropdown(SessionContext.CurrentUser.affectedSide);
            
            if (_txtSurgeryDate != null &&
                !string.IsNullOrEmpty(SessionContext.CurrentUser.surgeryDate))
                _txtSurgeryDate.value = SessionContext.CurrentUser.surgeryDate;
        }

        private AffectedSide MapDropdownToEnum(string dropdownValue)
        {
            if (string.IsNullOrEmpty(dropdownValue)) return AffectedSide.Unknown;

            string val = dropdownValue.ToLower();
            if (val.Contains("esquerd")) return AffectedSide.Left;
            if (val.Contains("direit")) return AffectedSide.Right;
            if (val.Contains("bilateral") || val.Contains("ambos")) return AffectedSide.Bilateral;

            return AffectedSide.Unknown;
        }

        private string MapEnumToDropdown(AffectedSide side)
        {
            return side switch
            {
                AffectedSide.Left => "Esquerdo",
                AffectedSide.Right => "Direito",
                AffectedSide.Bilateral => "Bilateral",
                _ => "Indefinido"
            };
        }

        private void OnSaveClicked(ClickEvent evt) => HandleSaveAsync();
    }
}
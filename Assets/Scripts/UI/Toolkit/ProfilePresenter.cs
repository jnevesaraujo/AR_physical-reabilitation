using UnityEngine;
using UnityEngine.UIElements;
using Firebase.Auth;
using App.Services;
using App.Data.Models;
using UserProfile = App.Data.Models.UserProfile;
using System;

namespace App.UI.Toolkit
{
    public class ProfilePresenter : MonoBehaviour
    {
        private VisualElement _root;
        private TextField _txtSubjectId, _txtEmail, _txtSurgeryDate;
        private DropdownField _drpAffectedSide;
        private Button _btnSave, _btnChangePass;

        private ProfileService _profileService;

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

            _txtSubjectId = _root.Q<TextField>("profile_subject_id");
            _txtEmail = _root.Q<TextField>("profile_email");
            _txtSurgeryDate = _root.Q<TextField>("profile_surgery_date");
            _drpAffectedSide = _root.Q<DropdownField>("profile_affected_side");

            _btnSave = _root.Q<Button>("btn_save_profile");
            _btnChangePass = _root.Q<Button>("btn_change_password");

            var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser != null && _txtEmail != null)
            {
                _txtEmail.value = currentUser.Email;
            }

            if (_btnSave != null)
            {
                Debug.Log("[ProfilePresenter] Save button found. Subscribing to click event.");
                _btnSave.RegisterCallback<ClickEvent>(_ => HandleSaveAsync());
                Debug.Log("[ProfilePresenter] Save button click event subscribed.");
            }
            if (_btnChangePass != null)
            {
                _btnChangePass.RegisterCallback<ClickEvent>(_ => HandleChangePassword());
            }
        }

        private void OnDisable()
        {
            // clean subscriptions to avoid memory leaks
            if (_btnSave != null) _btnSave.clicked -= HandleSaveAsync;
            if (_btnChangePass != null) _btnChangePass.clicked -= HandleChangePassword;
        }

        private async void HandleSaveAsync()
        {
            if (_txtSubjectId == null || string.IsNullOrEmpty(_txtSubjectId.value))
            {
                Debug.LogWarning("[ProfilePresenter] O ID da Cobaia é obrigatório.");
                return;
            }

            _btnSave.SetEnabled(false);
            _btnSave.text = "A guardar...";

            // Normalize the subject ID to uppercase and replace spaces with underscores
            string userId = _txtSubjectId.value.ToUpper().Replace(" ", "_");

            // Create profile object with the provided data
            UserProfile profile = new UserProfile
            {
                userId = userId,
                /*                 firstName = "Cobaia", 
                                lastName = "Teste",  */
                email = _txtEmail.value,
                registrationDate = DateTime.Now,
                totalSessionsCompleted = 0,
                affectedSide = _drpAffectedSide != null ? _drpAffectedSide.value : "Indefinido",
                surgeryDate = _txtSurgeryDate != null ? _txtSurgeryDate.value : "Não especificado"
            };

            try
            {
                await _profileService.CreateProfileAsync(profile);
                _btnSave.text = "Guardado com Sucesso!";
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProfilePresenter] Erro ao gravar Firestore: {e.Message}");
                _btnSave.text = "Erro. Tente Novamente.";
            }

            // Holds the success message for 2 seconds before resetting the button text and re-enabling it
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
    }
}
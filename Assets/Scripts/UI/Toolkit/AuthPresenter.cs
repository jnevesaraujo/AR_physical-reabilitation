using UnityEngine;
using UnityEngine.UIElements;
using App.Services;
using Firebase;
using Firebase.Extensions;
using App.Core;
using System;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument), typeof(UINavigationManager))]
    public class AuthPresenter : MonoBehaviour
    {
        private AuthService _auth;
        private ProfileService _profileService;
        private UINavigationManager _navManager;
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _navManager = GetComponent<UINavigationManager>();
            _profileService = new ProfileService();

            _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
            BindAuthUI();

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    _auth = new AuthService();
                    _auth.Initialize();

                }
            });
        }

        private void BindAuthUI()
        {
            // Internal navigation
            _root.Q<Label>("dontHaveAnAccount")?.RegisterCallback<ClickEvent>(_ => _navManager.NavigateTo(AppScreen.Register));
            _root.Q<Button>("btn_go_forgot")?.RegisterCallback<ClickEvent>(_ => _navManager.NavigateTo(AppScreen.ForgotPassword));
            _root.Q<Button>("btn_back_to_login")?.RegisterCallback<ClickEvent>(_ => _navManager.NavigateTo(AppScreen.Login));

            // Login form
            _root.Q<Button>("btn_login")?.RegisterCallback<ClickEvent>(_ =>
            {
                _root.focusController.focusedElement?.Blur();

                var email = _root.Q<TextField>("login_email")?.value;
                var password = _root.Q<TextField>("login_password")?.value;
                HandleLogin(email, password);
            });

            // Registration form
            _root.Q<Button>("btn_register")?.RegisterCallback<ClickEvent>(_ =>
            {
                var firstName = _root.Q<TextField>("register_firstname")?.value;
                var lastName = _root.Q<TextField>("register_lastname")?.value;
                var email = _root.Q<TextField>("register_email")?.value;
                var password = _root.Q<TextField>("register_password")?.value;
                var confirm = _root.Q<TextField>("register_confirmPassword")?.value;

                if (password != confirm)
                {
                    var error = _root.Q<Label>("register_error");
                    if (error != null) error.text = "As palavras-passe não coincidem.";
                    return;
                }

                HandleRegister(email, password, firstName, lastName);
            });
        }

        /*         private void HandleLogin(string email, string password)
                {
                    _auth.LoginAsync(email, password).ContinueWithOnMainThread(async task =>
                    {
                        if (task.IsCompletedSuccessfully && task.Result)
                        {
                            SessionContext.CurrentUser = await _profileService.LoadProfileAsync(_auth.UserId);
                            UpdateUsernameUI();
                            _navManager.NavigateTo(AppScreen.Home);
                        }
                        else
                        {
                            var errorLabel = _root.Q<Label>("login_error");
                            if (errorLabel != null) errorLabel.text = "Email ou palavra-passe inválidos.";
                        }
                    });
                } */

        private void HandleLogin(string email, string password)
        {
            Debug.Log($"[Auth] Attempting login for {email}");

            _auth.LoginAsync(email, password).ContinueWithOnMainThread(async task =>
            {
                Debug.Log($"[Auth] Task completed. Success={task.IsCompletedSuccessfully} " +
                          $"Result={task.Result} Faulted={task.IsFaulted}");

                if (task.IsFaulted)
                    Debug.LogError($"[Auth] Task faulted: {task.Exception}");

                if (task.IsCompletedSuccessfully && task.Result)
                {
                    Debug.Log($"[Auth] Login success, loading profile for {_auth.UserId}");

                    try
                    {
                        SessionContext.CurrentUser = await _profileService.LoadProfileAsync(_auth.UserId);
                        Debug.Log($"[Auth] Profile loaded: {SessionContext.CurrentUser?.firstName}");

                        UpdateUsernameUI();
                        Debug.Log("[Auth] Navigating to Home");
                        _navManager.NavigateTo(AppScreen.Home);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Auth] Profile load failed: {e.Message}\n{e.StackTrace}");
                        var errorLabel = _root.Q<Label>("login_error");
                        if (errorLabel != null)
                            errorLabel.text = "Erro ao carregar perfil. Tente novamente.";
                    }
                }
                else
                {
                    Debug.Log("[Auth] Login returned false or task not successful");
                    var errorLabel = _root.Q<Label>("login_error");
                    if (errorLabel != null) errorLabel.text = "Email ou palavra-passe inválidos.";
                }
            });
        }

        private void HandleRegister(string email, string password, string firstName, string lastName)
        {
            _auth.RegisterAsync(email, password, firstName, lastName).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled || !task.Result)
                {
                    var errorLabel = _root.Q<Label>("register_error");
                    if (errorLabel != null) errorLabel.text = "O registo falhou. Tente novamente.";
                    return;
                }

                _navManager.NavigateTo(AppScreen.Home);
            });
        }

        private void UpdateUsernameUI()
        {
            var home_lbl_username = _root.Q<Label>("home_lbl_username");
            var profile_lbl_username = _root.Q<Label>("profile_lbl_username");

            home_lbl_username.text = $"Olá {SessionContext.CurrentUser.firstName}!";
            profile_lbl_username.text = $"{SessionContext.CurrentUser.firstName} {SessionContext.CurrentUser.lastName}";

            home_lbl_username.MarkDirtyRepaint();
            profile_lbl_username.MarkDirtyRepaint();
        }

        private void OnRootPointerDown(PointerDownEvent evt)
        {
            // if the click is outside of a TextField, blur the currently focused element to dismiss the on-screen keyboard
            if (!(evt.target is TextField))
            {
                _root.focusController.focusedElement?.Blur();
            }
        }
    }
}
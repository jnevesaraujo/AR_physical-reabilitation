using UnityEngine;
using UnityEngine.UIElements;
using App.Services;
using Firebase;
using Firebase.Extensions;
using Services;

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
                Debug.Log("[AuthPresenter] Login button clicked");
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

        private void HandleLogin(string email, string password)
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
    }
}
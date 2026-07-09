using UnityEngine;
using App.Services;
using App.Core;
using Firebase;
using Firebase.Extensions;
using App.UI.Toolkit;
using Firebase.Auth;
using System;
using System.Threading.Tasks;

namespace App.Controllers
{
    [RequireComponent(typeof(AuthView), typeof(UINavigationManager))]
    public class AuthController : MonoBehaviour
    {
        private AuthView _view;
        private UINavigationManager _navManager;
        private AuthService _authService;
        private ProfileService _profileService;

        private void Awake()
        {
            _view = GetComponent<AuthView>();
            _navManager = GetComponent<UINavigationManager>();
            _profileService = new ProfileService();

            // Subscribe to view events
            _view.OnLoginRequested += HandleLogin;
            _view.OnRegisterRequested += HandleRegister;
            _view.OnNavigateRegisterRequested += () => _navManager.NavigateTo(AppScreen.Register);
        }

        private void Start()
        {
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(async task =>
            {
                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError($"[Firebase] Falha de dependências: {task.Result}");
                    return;
                }

                _authService = new AuthService();
                _authService.Initialize();

                await ProcessAuthenticationStateAsync();
            });
        }

        private async Task ProcessAuthenticationStateAsync()
        {
            var fbUser = FirebaseAuth.DefaultInstance.CurrentUser;

            // if the user is not logged in, navigate to the login screen
            if (fbUser == null)
            {
                _navManager.NavigateTo(AppScreen.Login);
                return;
            }

            // profile restoration logic
            if (SessionContext.CurrentUser == null)
            {
                try
                {
                    SessionContext.CurrentUser = await _profileService.LoadProfileAsync(fbUser.UserId);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Auth] Erro ao restaurar perfil: {e.Message}");
                }
            }

            // ternary navigation based on whether the user has a profile
            AppScreen targetScreen = SessionContext.ReturnToExerciseMenu
                ? AppScreen.Summary
                : AppScreen.Home;

            _navManager.NavigateTo(targetScreen);
        }

        private void HandleLogin(string email, string password)
        {
            _view.ClearErrors();

            _authService.LoginAsync(email, password).ContinueWithOnMainThread(async task =>
            {
                if (task.IsCompletedSuccessfully && task.Result)
                {
                    try
                    {
                        SessionContext.CurrentUser = await _profileService.LoadProfileAsync(_authService.UserId);
                        _navManager.NavigateTo(AppScreen.Home);
                    }
                    catch (System.Exception)
                    {
                        _view.ShowError("login_error", "Erro ao carregar perfil.");
                    }
                }
                else
                {
                    _view.ShowError("login_error", "Email ou palavra-passe inválidos.");
                }
            });
        }

        private void HandleRegister(string email, string password, string firstName, string lastName)
        {
            _view.ClearErrors();

            _authService.RegisterAsync(email, password, firstName, lastName).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled || !task.Result)
                {
                    _view.ShowError("register_error", "O registo falhou.");
                    return;
                }
                _navManager.NavigateTo(AppScreen.Home);
            });
        }

        private void OnDestroy()
        {
            // Prevent Memory Leaks
            if (_view != null)
            {
                _view.OnLoginRequested -= HandleLogin;
                _view.OnRegisterRequested -= HandleRegister;
            }
        }
    }
}
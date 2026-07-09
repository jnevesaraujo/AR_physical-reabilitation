using UnityEngine;
using App.Services;
using App.Core;
using Firebase;
using Firebase.Extensions;
using App.UI.Toolkit;
using Firebase.Auth;
using System;

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
                if (task.Result == DependencyStatus.Available)
                {
                    _authService = new AuthService();
                    _authService.Initialize();

              
                var fbUser = FirebaseAuth.DefaultInstance.CurrentUser;

                // 1. O Firebase detetou uma sessão ativa? (Voltou dos exercícios ou reabriu a App)
                if (fbUser != null)
                {
                    Debug.Log($"[Auth] Sessão Firebase detetada para {fbUser.Email}");

                    // 2. O nosso perfil customizado perdeu-se no reload da cena? Vamos buscá-lo!
                    if (SessionContext.CurrentUser == null)
                    {
                        Debug.Log("[Auth] A restaurar UserProfile da base de dados...");
                        try
                        {
                            SessionContext.CurrentUser = await _profileService.LoadProfileAsync(fbUser.UserId);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[Auth] Erro ao restaurar perfil: {e.Message}");
                        }
                    }

                    // 3. O Perfil já está carregado. Roteamento automático!
                    if (SessionContext.ReturnToExerciseMenu)
                    {
                        SessionContext.ReturnToExerciseMenu = true;
                        _navManager.NavigateTo(AppScreen.Summary);
                    }
                    else
                    {
                        // Navegar para a Home (Isto vai forçar o HomePresenter a popular os dados!)
                        _navManager.NavigateTo(AppScreen.Home);
                    }
                }
                else
                {
                    // Não há ninguém logado. Garante que fica no ecrã de Login.
                    _navManager.NavigateTo(AppScreen.Login);
                }
            }
            });
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
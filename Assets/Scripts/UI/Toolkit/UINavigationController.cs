using System;
using System.Collections.Generic;
using App.Services;
using Firebase;
using Firebase.Extensions;
using Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Toolkit
{
    public enum AppScreen
    {
        Login,
        Register,
        ForgotPassword,
        Home,
        Profile,
        Exercises
    }

    public class UINavigationManager : MonoBehaviour
    {
        [SerializeField] private ExerciseMenuManager _exerciseMenuManager;
        private AuthService _auth;
        private ProfileService _profileService;
        private Dictionary<AppScreen, VisualElement> _screens;
        private AppScreen _currentScreen;
        private VisualElement _root, _rootContainer, _topBar, _bottomBar;
        /*         private String _firstname, _lastname,_email = "";
                private String _password = ""; */

        // Screens that show the app chrome (top/bottom bar)
        private static readonly HashSet<AppScreen> _appScreens = new()
        {
            AppScreen.Home,
            AppScreen.Profile,
            AppScreen.Exercises
        };
        
        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            // In your app initializer
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    _auth = new AuthService();
                    _auth.Initialize();
                }
            });

            _profileService = new ProfileService();

            _rootContainer = _root.Q<VisualElement>("RootContainer");

            // Register all screens by name
            _screens = new Dictionary<AppScreen, VisualElement>
            {
                { AppScreen.Login,          _root.Q("panel_login") },
                { AppScreen.Register,       _root.Q("panel_register") },
                { AppScreen.ForgotPassword, _root.Q("panel_forgotPassword") },
                { AppScreen.Home,           _root.Q("panel_home") },
                { AppScreen.Profile,        _root.Q("panel_profile") },
                { AppScreen.Exercises,      _root.Q("panel_exercises") }
            };

            _topBar = _root.Q("topBar");
            _bottomBar = _root.Q("bottomBar");

            // Wire bottom nav buttons
            _root.Q<VisualElement>("nav_home")?.RegisterCallback<ClickEvent>
                (_ => NavigateTo(AppScreen.Home));
            _root.Q<VisualElement>("nav_profile")?.RegisterCallback<ClickEvent>
                (_ => NavigateTo(AppScreen.Profile));
            _root.Q<VisualElement>("nav_exercises")?.RegisterCallback<ClickEvent>
                (_ => NavigateTo(AppScreen.Exercises));

            // Wire auth buttons
            _root.Q<Label>("dontHaveAnAccount")?.RegisterCallback<ClickEvent>
                (_ => NavigateTo(AppScreen.Register));
            _root.Q<Button>("btn_go_forgot")?.RegisterCallback<ClickEvent>
                (_ => NavigateTo(AppScreen.ForgotPassword));
            _root.Q<Button>("btn_back_to_login")?.RegisterCallback<ClickEvent>
                (_ => NavigateTo(AppScreen.Login));
            /*             _root.Q<Button>("btn_login")?.RegisterCallback<ClickEvent>
                            (_ => NavigateTo(AppScreen.Profile)); */ // Simulate successful login

            _root.Q<Button>("btn_register")?.RegisterCallback<ClickEvent>
                (_ =>
                {
                    var firstName = _root.Q<TextField>("register_firstname")?.value;
                    var lastName = _root.Q<TextField>("register_lastname")?.value;
                    var email = _root.Q<TextField>("register_email")?.value;
                    var password = _root.Q<TextField>("register_password")?.value;
                    var confirm = _root.Q<TextField>("register_confirmPassword")?.value;
                    Debug.Log($"UI fields - email:{email} firstName:{firstName} lastName:{lastName} password:{(password == null ? "NULL" : "set")}");

                    if (password != confirm)
                    {
                        var error = _root.Q<Label>("register_error");
                        if (error != null) error.text = "Passwords don't match.";
                        return;
                    }

                    HandleRegister(email, password, firstName, lastName);
                    Debug.Log($"Attempting registration with {email}, {firstName} {lastName}");
                });

            _root.Q<Button>("btn_login")?.RegisterCallback<ClickEvent>
                (_ =>
                {
                    var email = _root.Q<TextField>("login_email")?.value;
                    var password = _root.Q<TextField>("login_password")?.value;

                    HandleLogin(email, password);
                });

            // Exercises screen buttons
            _root.Q<VisualElement>("btn-neckRotation")?.RegisterCallback<ClickEvent>
                (_ => _exerciseMenuManager.OnClick_LaunchNeckRotation());
            _root.Q<VisualElement>("btn-handGrip")?.RegisterCallback<ClickEvent>
                (_ => _exerciseMenuManager.OnClick_LaunchHandGrip());
            _root.Q<VisualElement>("btn-shoulderSlide")?.RegisterCallback<ClickEvent>
                (_ => _exerciseMenuManager.OnClick_LaunchShoulderSlide());

            if (SessionContext.ReturnToExerciseMenu)
            {
                SessionContext.ReturnToExerciseMenu = false; // reset flag
                NavigateTo(AppScreen.Exercises);
            }
            /*             else if (SessionContext.IsLoggedIn)
                        {
                            NavigateTo(AppScreen.Home);
                        } */
            else
            {   // Start at login
                NavigateTo(AppScreen.Login);
            }

        }


        public void NavigateTo(AppScreen screen)
        {
            // 1. Ocultar e desativar todos os ecrãs
            foreach (var kvp in _screens)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.style.display = DisplayStyle.None;
                    kvp.Value.SetEnabled(false);
                }
            }

            bool isAppScreen = _appScreens.Contains(screen);

            if (_rootContainer != null)
            {
                _rootContainer.style.display = isAppScreen ? DisplayStyle.Flex : DisplayStyle.None;
                Debug.Log($"Root container display set to {(isAppScreen ? "Flex" : "None")} for screen {screen}");
            }

            if (_screens.TryGetValue(screen, out var target) && target != null)
            {
                target.style.display = DisplayStyle.Flex;
                target.SetEnabled(true);
                target.style.flexGrow = 1;

                _currentScreen = screen;
            }

            if (_topBar != null)
                _topBar.style.display = isAppScreen ? DisplayStyle.Flex : DisplayStyle.None;

            if (_bottomBar != null)
                _bottomBar.style.display = isAppScreen ? DisplayStyle.Flex : DisplayStyle.None;

            UpdateBottomNavState(screen);
        }

        private void UpdateBottomNavState(AppScreen screen)
        {
            // Remove active class from all nav buttons
            _root.Q("nav_home")?.RemoveFromClassList("nav-active");
            _root.Q("nav_profile")?.RemoveFromClassList("nav-active");
            _root.Q("nav_exercises")?.RemoveFromClassList("nav-active");

            // Add to current
            string activeBtn = screen switch
            {
                AppScreen.Home => "nav_home",
                AppScreen.Profile => "nav_profile",
                AppScreen.Exercises => "nav_exercises",
                _ => null
            };

            if (activeBtn != null)
                _root.Q(activeBtn)?.AddToClassList("nav-active");
        }

        private void OnDisable()
        {
            // UIToolkit cleans up callbacks automatically when elements are destroyed
            // but if you need manual cleanup, do it here
        }

        // In your login button handler
        private void HandleLogin(string email, string password)
        {
            _auth.LoginAsync(email, password)
                .ContinueWithOnMainThread(async task =>
                {
                    if (task.IsCompletedSuccessfully && task.Result)
                    {
                        // Load profile from Firestore into SessionContext
                        var profileService = new ProfileService();
                        SessionContext.CurrentUser = await profileService
                            .LoadProfileAsync(_auth.UserId);

                        UpdateProfileUI();
                        
                        NavigateTo(AppScreen.Home);
                    }
                    else
                    {
                        var errorLabel = _root.Q<Label>("login_error");
                        if (errorLabel != null)
                            errorLabel.text = "Invalid email or password.";
                    }
                });
        }

        private void HandleRegister(string email, string password, string firstName, string lastName)
        {
            _auth.RegisterAsync(email, password, firstName, lastName)
                .ContinueWithOnMainThread(task =>
                {
                    // Check for faulted task first
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError($"[Nav] Register task faulted: {task.Exception?.Message}");
                        var errorLabel = _root.Q<Label>("register_error");
                        if (errorLabel != null)
                            errorLabel.text = "Registration failed. Try again.";
                        return;
                    }

                    if (task.Result)
                    {
                        NavigateTo(AppScreen.Home);
                    }
                    else
                    {
                        var errorLabel = _root.Q<Label>("register_error");
                        if (errorLabel != null)
                            errorLabel.text = "Registration failed. Try again.";
                    }
                });
        }

        private void UpdateProfileUI()
        {
            var profile_lbl_username = _root.Q<Label>("profile_lbl_username");

            if (profile_lbl_username != null && SessionContext.CurrentUser != null)
            {
                profile_lbl_username.text = $"Olá {SessionContext.CurrentUser.firstName}!";
            }
            else
            {
                Debug.LogWarning("[NavManager] Falha ao atualizar UI: Label ausente ou Contexto nulo.");
            }
        }
    }
}
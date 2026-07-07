using System;
using System.Collections.Generic;
using App.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Toolkit
{
    public enum AppScreen
    {
        Login, Register, ForgotPassword, Home, Profile, Exercises, Summary, Instructions
    }

    public class UINavigationManager : MonoBehaviour
    {
        private Dictionary<AppScreen, VisualElement> _screens;
        private VisualElement _root, _rootContainer, _topBar, _bottomBar;
        private AppScreen _currentScreen;
        public event Action<AppScreen> OnNavigatedTo;

        private static readonly HashSet<AppScreen> _appScreens = new()
        {
            AppScreen.Home, AppScreen.Profile, AppScreen.Exercises, AppScreen.Summary, AppScreen.Instructions
        };

        private void Awake()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _rootContainer = _root.Q<VisualElement>("RootContainer");
            _topBar = _root.Q("topBar");
            _bottomBar = _root.Q("bottomBar");

            _screens = new Dictionary<AppScreen, VisualElement>
            {
                { AppScreen.Login,          _root.Q("panel_login") },
                { AppScreen.Register,       _root.Q("panel_register") },
                { AppScreen.ForgotPassword, _root.Q("panel_forgotPassword") },
                { AppScreen.Home,           _root.Q("panel_home") },
                { AppScreen.Profile,        _root.Q("panel_profile") },
                { AppScreen.Exercises,      _root.Q("panel_exercises") },
                { AppScreen.Summary,        _root.Q("panel_summary") },
                { AppScreen.Instructions,   _root.Q("panel_instructions") }
            };

            BindNavigationBar();

            // Initial screen
            if (SessionContext.ReturnToExerciseMenu)
                NavigateTo(AppScreen.Summary);
            else
                NavigateTo(AppScreen.Login);
        }

        private void BindNavigationBar()
        {
            _root.Q<VisualElement>("nav_home")?.RegisterCallback<ClickEvent>(_ => NavigateTo(AppScreen.Home));
            _root.Q<VisualElement>("nav_profile")?.RegisterCallback<ClickEvent>(_ => NavigateTo(AppScreen.Profile));
            _root.Q<VisualElement>("nav_exercises")?.RegisterCallback<ClickEvent>(_ => NavigateTo(AppScreen.Exercises));
        }

        public void NavigateTo(AppScreen screen)
        {
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
                _rootContainer.style.display = isAppScreen ? DisplayStyle.Flex : DisplayStyle.None;

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

            OnNavigatedTo?.Invoke(screen);
            UpdateBottomNavState(screen);
        }

        private void UpdateBottomNavState(AppScreen screen)
        {
            _root.Q("nav_home")?.RemoveFromClassList("nav-active");
            _root.Q("nav_profile")?.RemoveFromClassList("nav-active");
            _root.Q("nav_exercises")?.RemoveFromClassList("nav-active");

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
        
    }
}
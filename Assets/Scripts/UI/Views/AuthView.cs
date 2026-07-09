using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Views
{
    [RequireComponent(typeof(UIDocument))]
    public class AuthView : MonoBehaviour
    {
        private VisualElement _root;
        
        // Controller subscribes to these events to handle user actions
        public event Action<string, string> OnLoginRequested;
        public event Action<string, string, string, string> OnRegisterRequested;
        public event Action OnNavigateRegisterRequested;
        //public event Action OnNavigateHomeRequested;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            BindUI();
        }

        private void BindUI()
        {
        
            _root.Q<Label>("dontHaveAnAccount")?.RegisterCallback<ClickEvent>(_ => OnNavigateRegisterRequested?.Invoke());

            _root.Q<Button>("btn_login")?.RegisterCallback<ClickEvent>(_ =>
            {
                var email = _root.Q<TextField>("login_email")?.value;
                var password = _root.Q<TextField>("login_password")?.value;
                OnLoginRequested?.Invoke(email, password);
            });

            _root.Q<Button>("btn_register")?.RegisterCallback<ClickEvent>(_ =>
            {
                var firstName = _root.Q<TextField>("register_firstname")?.value;
                var lastName = _root.Q<TextField>("register_lastname")?.value;
                var email = _root.Q<TextField>("register_email")?.value;
                var password = _root.Q<TextField>("register_password")?.value;
                var confirm = _root.Q<TextField>("register_confirmPassword")?.value;

                if (password != confirm)
                {
                    ShowError("register_error", "As palavras-passe não coincidem.");
                    return;
                }

                OnRegisterRequested?.Invoke(email, password, firstName, lastName);
            });
        }

        // Public methods to show errors and clear them, which the Controller can call
        public void ShowError(string elementId, string message)
        {
            var errorLabel = _root.Q<Label>(elementId);
            if (errorLabel != null) errorLabel.text = message;
        }

        public void ClearErrors()
        {
            ShowError("login_error", string.Empty);
            ShowError("register_error", string.Empty);
        }
    }
}
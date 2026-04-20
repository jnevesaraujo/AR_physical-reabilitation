using UnityEngine;
using UnityEngine.SceneManagement;

namespace App.UI
{
    public class NavigationManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject loginPanel;
        public GameObject registerPanel;
        public GameObject mainMenuPanel;

        public static bool isReturningFromSession = false;

        void Start()
        {
            if (loginPanel != null && mainMenuPanel != null) 
            {
                if (isReturningFromSession)
                {
                    ShowMainMenu();
                    isReturningFromSession = false; 
                }
                else
                {
                    ShowLogin();
                }
            }
        }

        // --- Scene Transitions ---

        public void LoadExerciseScene()
        {
            SceneManager.LoadScene("App_Exercise"); 
        }

        public void ReturnToMainMenu()
        {
            isReturningFromSession = true; 
            SceneManager.LoadScene("App_Menu");
        }

        // --- Panels ---

        public void ShowLogin()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(false);
        }

        public void ShowMainMenu()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (registerPanel != null) registerPanel.SetActive(false);
        }

        public void ShowRegistration()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (registerPanel != null) registerPanel.SetActive(true);
        }
    }
}
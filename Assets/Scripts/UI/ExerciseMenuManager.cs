using UnityEngine;
using UnityEngine.SceneManagement;
using App.Data.ScriptableObjects;
using App.Core;
using App.UI.Toolkit;

namespace App.UI
{
    public class ExerciseMenuManager : MonoBehaviour
    {

        [Header("Exercise Definitions")]
        public NeckRotationDefinition neckRotationData;
        public HandGripDefinition handGripData;
        public ShoulderSlideDefinition shoulderSlideData;
        public ElbowFlexionDefinition elbowSlideData;
        private UINavigationManager _navigationManager;
        private InstructionsPresenter _instructionsPresenter;

        [Header("Destination Scene")]
        [Tooltip("Scene name where the ExerciseAppManager is located")]
        public string arSceneName = "App_Exercise";

        void Start()
        {
            _navigationManager = FindFirstObjectByType<UINavigationManager>();
            _instructionsPresenter = FindFirstObjectByType<InstructionsPresenter>();
        }
        public void OnClick_LaunchNeckRotation()
        {
            Debug.Log("[MainMenu] A preparar Rotação Cervical...");

            SessionContext.CurrentExercise = neckRotationData;
            loadInstructionsScreen();
            /* SessionContext.TargetARScene = arSceneName;
            _instructionsPresenter.PopulateFromSession();
            _navigationManager.NavigateTo(AppScreen.Instructions); */
            //SceneManager.LoadScene(arSceneName);
        }

        public void OnClick_LaunchHandGrip()
        {
            Debug.Log("[MainMenu] A preparar Exercicio Palmar (Hand Grip)...");

            SessionContext.CurrentExercise = handGripData;
            loadInstructionsScreen();
            /* SessionContext.TargetARScene = arSceneName;
            _instructionsPresenter.PopulateFromSession();
            _navigationManager.NavigateTo(AppScreen.Instructions); */
            //SceneManager.LoadScene(arSceneName);
        }

        public void OnClick_LaunchShoulderSlide()
        {
            Debug.Log("[MainMenu] A preparar Deslizar do Ombro...");

            SessionContext.CurrentExercise = shoulderSlideData;
            loadInstructionsScreen();
            /* SessionContext.TargetARScene = arSceneName;
            _instructionsPresenter.PopulateFromSession();
            _navigationManager.NavigateTo(AppScreen.Instructions); */
            //SceneManager.LoadScene(arSceneName);
        }

        public void OnClick_LaunchElbowFlexion()
        {
            Debug.Log("[MainMenu] A preparar Flexão do Cotovelo...");

            SessionContext.CurrentExercise = elbowSlideData;
            loadInstructionsScreen();
            /* SessionContext.TargetARScene = arSceneName;
            _instructionsPresenter.PopulateFromSession();
            _navigationManager.NavigateTo(AppScreen.Instructions); */
            //SceneManager.LoadScene(arSceneName);
        }

        private void loadInstructionsScreen()
        {
            SessionContext.TargetARScene = arSceneName;
            _instructionsPresenter.PopulateFromSession();
            _navigationManager.NavigateTo(AppScreen.Instructions);
        }

    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using App.Data.ScriptableObjects;
using App.Core;

namespace App.UI
{
    public class ExerciseMenuManager : MonoBehaviour
    {

        [Header("Exercise Definitions")]
        public NeckRotationDefinition neckRotationData;
        public HandGripDefinition handGripData;
        public ShoulderSlideDefinition shoulderSlideData;
        public ElbowFlexionDefinition elbowSlideData;

        [Header("Destination Scene")]
        [Tooltip("Scene name where the ExerciseAppManager is located")]
        public string arSceneName = "App_Exercise";

        public void OnClick_LaunchNeckRotation()
        {
            Debug.Log("[MainMenu] A preparar Rotação Cervical...");

            SessionContext.CurrentExercise = neckRotationData;
            SceneManager.LoadScene(arSceneName);
        }

        public void OnClick_LaunchHandGrip()
        {
            Debug.Log("[MainMenu] A preparar Exercicio Palmar (Hand Grip)...");

            SessionContext.CurrentExercise = handGripData;
            SceneManager.LoadScene(arSceneName);
        }

        public void OnClick_LaunchShoulderSlide()
        {
            Debug.Log("[MainMenu] A preparar Deslizar do Ombro...");

            SessionContext.CurrentExercise = shoulderSlideData;
            SceneManager.LoadScene(arSceneName);
        }

        public void OnClick_LaunchElbowFlexion()
        {
            Debug.Log("[MainMenu] A preparar Flexão do Cotovelo...");

            SessionContext.CurrentExercise = elbowSlideData;
            SceneManager.LoadScene(arSceneName);
        }

    }
}
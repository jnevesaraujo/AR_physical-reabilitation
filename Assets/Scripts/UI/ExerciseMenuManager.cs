using UnityEngine;
using UnityEngine.SceneManagement;
using App.Data.ScriptableObjects;
using Services;

namespace App.UI
{
    public class ExerciseMenuManager : MonoBehaviour
    {

        [Header("Configuração dos Exercícios")]
        public NeckRotationDefinition neckRotationData;
        public HandGripDefinition handGripData;

        [Header("Cena de Destino")]
        [Tooltip("O nomeda cena onde está o ExerciseAppManager")]
        public string arSceneName = "App_Exercise";

        public void OnClick_LaunchNeckRotation()
        {
            Debug.Log("[MainMenu] A preparar Rotação Cervical...");

            SessionContext.CurrentExercise = neckRotationData;

            SceneManager.LoadScene(arSceneName);
        }

        public void OnClick_LaunchHandGrip()
        {
            Debug.Log("[MainMenu] A preparar Preensão Palmar (Hand Grip)...");

            SessionContext.CurrentExercise = handGripData;
            SceneManager.LoadScene(arSceneName);
        }
    }
}
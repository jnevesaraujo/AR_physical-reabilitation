using UnityEngine;
using App.Data.ScriptableObjects;
using App.UI;
using App.Vision;
using Services;
using App.Vision.Extractors; // Onde se encontra o SessionContext

namespace App.Core
{
    public class ExerciseAppManager : MonoBehaviour
    {
        [Header("UI & Visuals")]
        [SerializeField] private ExerciseHUD exerciseHUD;
        [SerializeField] private ARExerciseVisualizer visualizer;

        [Header("MediaPipe Extraction & Evaluation")]
        [SerializeField] private PoseDataExtractor poseExtractor;
        [SerializeField] private HandDataExtractor handExtractor;
        [Header("Solution Objects)")]
        public GameObject solutionPose;
        public GameObject solutionHand;

        [Header("Debug / Editor Only")]
        [SerializeField] private ExerciseDefinition debugExerciseDefinition;

        private ExerciseDefinition _activeExercise;

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Start()
        {
            _activeExercise = SessionContext.CurrentExercise;

#if UNITY_EDITOR
            // Injeção de dependência para testes no Editor
            if (_activeExercise == null && debugExerciseDefinition != null)
            {
                _activeExercise = debugExerciseDefinition;
                Debug.LogWarning("Modo Editor: A inicializar com o debugExerciseDefinition.");
            }
#endif

            if (_activeExercise == null)
            {
                Debug.LogError("Nenhum exercício selecionado no contexto. Encaminhar para Menu...");
                return;
            }

            if (exerciseHUD != null)
                exerciseHUD.InitializeHUD(_activeExercise.targetRepetitions);

            ConfigureVisionPipeline(_activeExercise);
        }

        private void ConfigureVisionPipeline(ExerciseDefinition exercise)
        {
            // Desativar tudo por defeito
            poseExtractor.gameObject.SetActive(false);
            handExtractor.gameObject.SetActive(false);
            /* solutionPose.SetActive(false);
            solutionHand.SetActive(false); */

            if (exercise.requiredTrackingModel == ExerciseDefinition.TrackingModelType.BodyPose)
            {   
                solutionPose.SetActive(true);
                poseExtractor.gameObject.SetActive(true);
                poseExtractor.Initialize(exercise as NeckRotationDefinition, exerciseHUD, visualizer);
            }
            else if (exercise.requiredTrackingModel == ExerciseDefinition.TrackingModelType.HandsOnly)
            {
                solutionHand.SetActive(true);
                handExtractor.gameObject.SetActive(true);
                handExtractor.Initialize(exercise, exerciseHUD, visualizer);
            }
        }

        private void OnDestroy()
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }
}
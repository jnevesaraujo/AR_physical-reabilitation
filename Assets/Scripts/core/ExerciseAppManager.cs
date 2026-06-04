using UnityEngine;
using App.Data.ScriptableObjects;
using App.UI;
using App.Vision;
using App.Vision.Extractors;
using System; // Onde se encontra o SessionContext

namespace App.Core
{
    public class ExerciseAppManager : MonoBehaviour
    {
        [Header("UI & Visuals")]
        [SerializeField] private ExerciseHUD exerciseHUD;
        [SerializeField] private ARExerciseVisualizer visualizer;

        [Header("MediaPipe Extraction & Evaluation")]
        [SerializeField] private NeckRotationExtractor neckExtractor;
        [SerializeField] private HandGripExtractor handExtractor;
        [SerializeField] private ShoulderSlideExtractor shoulderExtractor;
        [SerializeField] private ElbowFlexionExtractor elbowExtractor;
        [Header("Solution Objects)")]
        public GameObject solutionPose;
        public GameObject solutionHand;
        public GameObject annotatableScreenPose;
        public GameObject annotatableScreenHand;

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
                Debug.LogWarning("[ExerciseAppManager] Modo Editor: A inicializar com o debugExerciseDefinition.");
            }
#endif

            if (_activeExercise == null)
            {
                Debug.LogError("[ExerciseAppManager] Nenhum exercício selecionado no contexto. Encaminhar para Menu...");
                return;
            }

            exerciseHUD.InitializeHUD(_activeExercise.targetRepetitions);
            ConfigureVisionPipeline(_activeExercise);
        }

        private void ConfigureVisionPipeline(ExerciseDefinition exercise)
        {
            DisableMediaPipeExtractors();
            DisableMediaPipeVisuals();

            if (exercise.requiredTrackingModel == ExerciseDefinition.TrackingModelType.BodyPose)
            {
                Debug.Log("<color=green>[AppManager] A ligar o motor da pose!</color>");
                enableMediaPipePoseVisuals();
                if (exercise is NeckRotationDefinition)
                {
                    enableMediaPipeExtractor("Neck");
                    neckExtractor.Initialize(exercise as NeckRotationDefinition, exerciseHUD, visualizer);
                }
                else if (exercise is ShoulderSlideDefinition)
                {
                    enableMediaPipeExtractor("Shoulder");
                    shoulderExtractor.Initialize(exercise as ShoulderSlideDefinition, exerciseHUD, visualizer);
                }
                else if (exercise is ElbowFlexionDefinition)
                {
                    enableMediaPipeExtractor("Elbow");
                    elbowExtractor.Initialize(exercise as ElbowFlexionDefinition, exerciseHUD, visualizer);
                }
            }
            else if (exercise.requiredTrackingModel == ExerciseDefinition.TrackingModelType.HandsOnly)
            {
                Debug.Log("<color=blue>[AppManager] A ligar o motor da mão!</color>");
                enableMediaPipeHandVisuals();
                enableMediaPipeExtractor("Hand");
                handExtractor.Initialize(exercise, exerciseHUD, visualizer);
            }
        }

        private void DisableMediaPipeExtractors()
        {
            neckExtractor.gameObject.SetActive(false);
            handExtractor.gameObject.SetActive(false);
            shoulderExtractor.gameObject.SetActive(false);
            elbowExtractor.gameObject.SetActive(false);
        }

        private void DisableMediaPipeVisuals()
        {
            solutionPose.SetActive(false);
            solutionHand.SetActive(false);
            annotatableScreenPose.SetActive(false);
            annotatableScreenHand.SetActive(false);
        }

        private void enableMediaPipePoseVisuals()
        {
            solutionPose.SetActive(true);
            annotatableScreenPose.SetActive(true);
        }

        private void enableMediaPipeHandVisuals()
        {
            solutionHand.SetActive(true);
            annotatableScreenHand.SetActive(true);
        }

        private void enableMediaPipeExtractor(String extractorName)
        {
            switch (extractorName)
            {
                case "Neck":
                    neckExtractor.gameObject.SetActive(true);
                    break;
                case "Hand":
                    handExtractor.gameObject.SetActive(true);
                    break;
                case "Shoulder":
                    shoulderExtractor.gameObject.SetActive(true);
                    break;
                case "Elbow":
                    elbowExtractor.gameObject.SetActive(true);
                    break;
                default:
                    Debug.LogWarning($"Extractor '{extractorName}' desconhecido. Nenhum extractor ativado.");
                    break;
            }
        }

        private void OnDestroy()
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }
}
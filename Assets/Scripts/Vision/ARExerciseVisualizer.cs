using UnityEngine;
using App.Data.ScriptableObjects;

namespace App.Vision
{
    public class ARExerciseVisualizer : MonoBehaviour
    {
        [Header("Visual Settings")]
        public float visualScaleMultiplier = 100f;
        public float zOffset = -2f;
        public float neckRadiusModifier = 1.5f;
        private GameObject _activeGuide;
        private ARGuideController _guideController;
        private Vector3 _anchorOffset;

        public void InitializeGuide(ExerciseDefinition def, Vector3 startPosition, float targetY = 0f)
        {
            Debug.Log($"[Visualizer] A iniciar guia para o tipo: {def.GetType().Name}");
            CleanUp();

            if (def is NeckRotationDefinition neckDef)
            {

                float visualRadius = neckDef.minimumRotationAmplitude * visualScaleMultiplier * neckRadiusModifier;

                if (neckDef.visualGuidePrefab != null)
                {
                    _activeGuide = Instantiate(neckDef.visualGuidePrefab);
                    _anchorOffset = new Vector3(0, 0, zOffset);

                    _activeGuide.transform.position = new Vector3(
                        startPosition.x,
                        startPosition.y,
                        startPosition.z + zOffset
                    );

                    _guideController = _activeGuide.GetComponent<ARGuideController>();
                    if (_guideController != null)
                    {
                        _guideController.InitializeGuide(visualRadius, neckDef.targetSecondsPerRep, startPosition);
                    }
                }
            }
            else if (def is HandGripDefinition handDef)
            {
                if (handDef.visualGuidePrefab != null)
                {
                    _activeGuide = Instantiate(handDef.visualGuidePrefab);

                    _activeGuide.transform.localScale = handDef.visualGuidePrefab.transform.localScale * visualScaleMultiplier;

                    _activeGuide.transform.position = new Vector3(startPosition.x, startPosition.y, startPosition.z + zOffset);

                    _guideController = _activeGuide.GetComponent<ARGuideController>();
                    if (_guideController != null)
                    {
                        _guideController.InitializeHandGuide(startPosition);
                    }
                }
            }
            else if (def is ShoulderSlideDefinition shoulderDef)
            {
                if (shoulderDef.visualGuidePrefab != null)
                {
                    _activeGuide = Instantiate(shoulderDef.visualGuidePrefab);
                    _activeGuide.transform.position = Vector3.zero; 

                    _guideController = _activeGuide.GetComponent<ARGuideController>();
                    if (_guideController != null)
                    {
                        Vector3 trackStartPos = new Vector3(startPosition.x, startPosition.y, startPosition.z + zOffset);
                        
                        _guideController.InitializeShoulderGuide(trackStartPos, targetY);
                    }
                }
            }

        }

        public void UpdatePacerFeedback(Vector3 currentNosePosition)
        {
            if (_guideController != null)
                _guideController.EvaluateSynchronization(currentNosePosition);
        }

        public void UpdateHandGripVisuals(Vector3 centerPosition, float apertureRatio, float holdProgress)
        {
            if (_guideController != null)
            {
                _guideController.UpdateEnergySphere(centerPosition, apertureRatio, holdProgress);
            }
        }

        public void UpdateShoulderSlideVisuals(Vector3 wristPos, float progress, bool isDiscovering)
        {
            if (_guideController != null)
            {
                // Usa o zOffset global da classe para manter a consistência
                _guideController.UpdateShoulderGuide(wristPos, progress, isDiscovering, zOffset);
            }
        }

        public void TriggerSuccessFeedback()
        {
            if (_guideController != null)
            {
                _guideController.PlaySuccessParticles();
            }
        }

        public void SetFeedbackMode(bool isCorrectPosture)
        {
            if (_guideController != null)
            {
                _guideController.SetColor(isCorrectPosture ? Color.green : Color.red);
            }
        }

        private void CleanUp()
        {
            if (_activeGuide != null) DestroyImmediate(_activeGuide);
            _guideController = null;
        }
    }
}
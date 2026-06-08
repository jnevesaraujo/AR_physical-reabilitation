/* using UnityEngine;
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

        public void InitializeGuide(ExerciseDefinition def, Vector3 startPosition,
                             float targetY = 0f, float visualRadius = 0f,
                             float armLength = 1f)
        {
            Debug.Log($"[Visualizer] A iniciar guia para o tipo: {def.GetType().Name}");
            CleanUp();

            if (def is NeckRotationDefinition neckDef)
            {

                //float visualRadius = neckDef.minimumRotationAmplitude * visualScaleMultiplier * neckRadiusModifier;

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
                        float radius = visualRadius > 0f ? visualRadius : neckDef.minimumRotationAmplitude * visualScaleMultiplier * neckRadiusModifier;

                        _guideController.InitializeGuide(radius, neckDef.targetSecondsPerRep, startPosition);
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
            else if (def is ElbowFlexionDefinition elbowDef)
            {
                if (elbowDef.visualGuidePrefab == null)
                {
                    Debug.LogError("[Visualizer] ElbowFlexionDefinition has no visualGuidePrefab.");
                    return;
                }

                _activeGuide = Instantiate(elbowDef.visualGuidePrefab);
                _activeGuide.transform.position = Vector3.zero;
                _guideController = _activeGuide.GetComponent<ARGuideController>();

                if (_guideController != null)
                {
                    // Z: stay at same plane as landmarks, small negative offset to render in front
                    Vector3 restPos = new Vector3(
                        startPosition.x,
                        startPosition.y,
                        startPosition.z - 5f);  // 5px in front in landmark space

                    _guideController.PlaceRestRing(restPos, armLength);
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

        // Called on first calibration tap (arm at rest)
        public void PlaceElbowRestRing(Vector3 wristPos, float armLength)
        {
            // Override zOffset for elbow — use relative depth, not fixed world units
            float z = wristPos.z - 0.05f;
            _guideController?.PlaceRestRing(
                new Vector3(wristPos.x, wristPos.y, z), armLength);
        }

        public void PlaceElbowPeakRing(Vector3 wristPos, float armLength)
        {
            _guideController?.PlacePeakRing(
                new Vector3(wristPos.x, wristPos.y, wristPos.z - 5f),
                armLength);
        }

        public void UpdateElbowRings(Vector3 wristPos, float progress)
        {
            _guideController?.UpdateElbowRings(wristPos, progress);
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
} */

using UnityEngine;
using App.Data.ScriptableObjects;
using App.Vision.Guides;

namespace App.Vision
{
    public class ARExerciseVisualizer : MonoBehaviour
    {
        private IExerciseGuide _activeGuide;

        // Called by extractor on first calibration
        public void InitializeGuide(ExerciseDefinition def, Vector3 anchorPos, float bodyScale)
        {
            CleanUp();

            if (def.visualGuidePrefab == null)
            {
                Debug.LogError($"[Visualizer] {def.GetType().Name} has no visualGuidePrefab.");
                return;
            }

            var go = Instantiate(def.visualGuidePrefab);
            _activeGuide = go.GetComponent<IExerciseGuide>();

            if (_activeGuide == null)
            {
                Debug.LogError($"[Visualizer] Prefab for {def.GetType().Name} " +
                               $"has no IExerciseGuide component.");
                Destroy(go);
                return;
            }

            _activeGuide.Initialize(anchorPos, bodyScale);
        }

        // Called by extractors that have a two-step calibration (ShoulderSlide, ElbowFlexion)
        public void PlacePeakMarker(Vector3 peakPos, float bodyScale)
        {
            _activeGuide?.PlacePeakMarker(peakPos, bodyScale);
        }

        // Called every frame
        public void UpdateVisuals(Vector3 trackedPos, float progress)
        {
            _activeGuide?.UpdateVisuals(trackedPos, progress);
        }

        // Called on rep completion
        public void TriggerSuccessFeedback()
        {
            _activeGuide?.PlaySuccess();
        }

        // Called on posture change
        public void SetFeedbackMode(bool isGood)
        {
            _activeGuide?.SetPostureFeedback(isGood);
        }

        private void CleanUp()
        {
            if (_activeGuide != null)
            {
                _activeGuide.Cleanup();
                // The guide's GameObject is the prefab instance — destroy it
                var mono = _activeGuide as MonoBehaviour;
                if (mono != null) Destroy(mono.gameObject);
                _activeGuide = null;
            }
        }

        public void UpdateHandGripVisuals(Vector3 palmCenter, float apertureRatio, float holdProgress)
        {
            (_activeGuide as HandGripGuide)?.UpdateHandGripVisuals(palmCenter, apertureRatio, holdProgress);
        }

        private void OnDestroy() => CleanUp();
    }
}
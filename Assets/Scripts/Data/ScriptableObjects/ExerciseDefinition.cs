using UnityEngine;

namespace App.Data.ScriptableObjects
{
    public abstract class ExerciseDefinition : ScriptableObject
    {
        public enum TrackingModelType
        {
            BodyPose,   // Loads MediaPipe Pose (33 points) - For Neck, Shoulder, Elbow
            HandsOnly   // Loads MediaPipe Hands (21 points per hand) - For Grip/Fingers
        }

        [Header("Identity")]
        public string exerciseId;
        public string exerciseName;
        [TextArea(3, 5)]
        public string description;

        [Header("Core System Requirements")]
        [Tooltip("Defines which model must be loaded into the device memory for this exercise.")]
        public TrackingModelType requiredTrackingModel = TrackingModelType.BodyPose;

        [Header("Base Settings")]
        public int targetRepetitions = 10;
        public float durationInSeconds = 60f;

        [Header("Gamification")]
        public int basePointsPerRep = 50;
        public int perfectFormBonus = 20; // TODO
        public string achievementIdToUnlock;

        [Header("Gamification Feedback")]
        [Tooltip("The 3D Prefab that will serve as the guide")]
        public GameObject visualGuidePrefab;

        [Tooltip("Where should this guide be anchored? (e.g., Nose, RightShoulder, Hand, ScreenCenter).")]
        public TrackingAnchor guideAnchor;

        [Tooltip("Offset from the anchor point.")]
        public Vector3 guideOffset = Vector3.zero;

        [Header("UI Reference")]
        public Sprite tutorialIcon;

        /*         public enum TrackingAnchor
                {
                    ScreenCenter,
                    Nose,
                    RightShoulder,
                    LeftShoulder,
                    RightHand,
                    LeftHand
                } */

        public enum TrackingAnchor
        {
            ScreenCenter,
            Nose,
            RightShoulder,
            LeftShoulder,
            RightWrist,
            LeftWrist,
            IndexFingerTip
        }
    }
}
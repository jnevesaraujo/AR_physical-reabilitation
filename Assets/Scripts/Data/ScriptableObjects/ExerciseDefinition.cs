using UnityEngine;

namespace App.Data.ScriptableObjects
{
    public abstract class ExerciseDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string exerciseId;
        public string exerciseName;
        [TextArea(3, 5)]
        public string description;

        [Header("Base Settings")]
        public int targetRepetitions = 10;
        public float durationInSeconds = 60f;

        [Header("Gamification")]
        public int basePointsPerRep = 50;
        public int perfectFormBonus = 20;
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

        public enum TrackingAnchor
        {
            ScreenCenter,
            Nose,
            RightShoulder,
            LeftShoulder,
            RightHand,
            LeftHand
        }
    }
}
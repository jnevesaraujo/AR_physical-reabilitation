using UnityEngine;

namespace App.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ShoulderSlideDef", menuName = "RehabApp/Exercises/Shoulder Slide")]
    public class ShoulderSlideDefinition : ExerciseDefinition
    {
        [Header("Arm Selection")]
        [Tooltip("True for the right arm, false for the left arm.")]
        public bool isRightArm = true;

        [Header("Biomechanical Parameters")]
        [Tooltip("Maximum allowed deviation for the sides (X-axis) in meters.")]
        public float horizontalTolerance = 0.15f; 
        
        [Tooltip("Time (in seconds) the hand must remain still at the top to register the amplitude.")]
        public float discoveryHoldTime = 1.0f;

        [Tooltip("Minimum amplitude required to register a successful discovery.")]
        public float minimumDiscoveryAmplitude = 0.5f;
    }
}
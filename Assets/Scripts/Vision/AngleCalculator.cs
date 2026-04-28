using UnityEngine;

namespace App.Vision
{
    /*
    * class AngleCalculator
    * Pure mathematical utility class for biomechanical calculations.
    * Fully stateless and testable.
    */
    public static class AngleCalculator
    {
        /// <summary>
        /// Calculates the angle in degrees (0 to 360) from a center point to a target point.
        /// Useful for circular motion tracking (like neck rotations).
        /// </summary>
        public static float CalculateAngle360(Vector2 center, Vector2 target)
        {
            // Vector pointing from center to target
            Vector2 direction = target - center;
            
            // Atan2 returns radians between -PI and PI.
            float angleInRadians = Mathf.Atan2(direction.y, direction.x);
            float angleInDegrees = angleInRadians * Mathf.Rad2Deg;
            
            // Normalize to 0-360 range for easier quadrant tracking
            if (angleInDegrees < 0)
            {
                angleInDegrees += 360f;
            }
            
            return angleInDegrees;
        }

        /// <summary>
        /// Calculates the absolute vertical distance between two points.
        /// Ideal for checking shoulder alignment.
        /// </summary>
        public static float GetVerticalDifference(Vector3 pointA, Vector3 pointB)
        {
            return Mathf.Abs(pointA.y - pointB.y);
        }

        public static float GetDepthDifference(Vector3 point1, Vector3 point2)
        {
            return Mathf.Abs(point1.z - point2.z);
        }
        
        /// <summary>
        /// Calculates the 2D distance between two points, ignoring depth (Z-axis).
        /// Useful for checking if a movement meets the minimum required amplitude.
        /// </summary>
        public static float GetDistance2D(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }
        public static float GetDistance3D(Vector3 origin, Vector3 target)
        {
            return Vector3.Distance(origin, target);
        }
    }
}
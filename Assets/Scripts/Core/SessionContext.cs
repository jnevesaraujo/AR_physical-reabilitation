using App.Data.Models;
using App.Data.ScriptableObjects;
using UnityEngine;

namespace App.Core
{
    public static class SessionContext
    {
        public static UserProfile CurrentUser { get; set; }
        public static ExerciseDefinition CurrentExercise { get; set; }
        public static int ElapsedSeconds { get; set; } = 0;
        public static int CurrentRepetitions { get; set; } = 0;
        public static bool ReturnToExerciseMenu { get; set; }
        public static string UserId => CurrentUser?.userId;
        public static bool IsLoggedIn => CurrentUser != null;
        public static bool debugMode = false;

        public static void ClearExerciseSession()
        {
            CurrentExercise = null;
            ElapsedSeconds = 0;
            CurrentRepetitions = 0;
        }

        public static void Clear()
        {
            ClearExerciseSession();
            CurrentUser = null;
        }
    }
}
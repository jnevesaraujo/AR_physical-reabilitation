using App.Data.Models;
using App.Data.ScriptableObjects;
using UnityEngine;

namespace Services
{
    public static class SessionContext
    {
        public static UserProfile CurrentUser { get; set; }
        public static ExerciseDefinition CurrentExercise { get; set; }
        public static bool ReturnToExerciseMenu { get; set; }
        public static string UserId => CurrentUser?.userId;
        public static bool IsLoggedIn => CurrentUser != null;

        public static void Clear()
        {
            CurrentExercise = null;
        }
    }
}
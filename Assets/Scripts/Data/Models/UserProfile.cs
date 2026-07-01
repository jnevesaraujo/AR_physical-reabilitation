using System;

namespace App.Data.Models
{
    [Serializable]
    public class UserProfile
    {
        public string userId;
        public string subjectId; // Unique identifier for the test_subjects
        public string firstName;
        public string lastName;
        public string email;
        public DateTime registrationDate;
        public int totalSessionsCompleted;
        public AffectedSide affectedSide;
        public string surgeryDate;

    }

    public enum AffectedSide
    {
        Unknown,
        Left,
        Right,
        Bilateral
    }
}
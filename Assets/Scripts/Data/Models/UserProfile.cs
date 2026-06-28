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
        public string affectedSide;
        public string surgeryDate;
    }
}
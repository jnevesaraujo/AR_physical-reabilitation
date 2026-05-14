using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Data.Models;

namespace App.Services
{
    public class SessionService
    {
        private FirebaseFirestore _db;
        private string _userId;

        public void Initialize(string userId)
        {
            _db = FirebaseFirestore.DefaultInstance;
            _userId = userId;
        }

        public async Task SaveSessionAsync(SessionRecord session)
        {
            var docRef = _db
                .Collection("users")
                .Document(_userId)
                .Collection("sessions")
                .Document();

            var data = new Dictionary<string, object>
            {
                { "exerciseId",  session.exerciseId },
                { "date",        session.sessionTimestamp.ToString("o") },
                { "repCount",    session.completedReps },
                { "accuracyScore", session.accuracyScore }
            };

            await docRef.SetAsync(data);
        }

        public async Task<List<SessionRecord>> GetSessionHistoryAsync()
        {
            var snapshot = await _db
                .Collection("users")
                .Document(_userId)
                .Collection("sessions")
                .OrderByDescending("date")
                .Limit(20)
                .GetSnapshotAsync();

            var sessions = new List<SessionRecord>();
            foreach (var doc in snapshot.Documents)
            {
                sessions.Add(new SessionRecord
                {
                    exerciseId  = doc.GetValue<string>("exerciseId"),
                    completedReps    = doc.GetValue<int>("completedReps"),
                    accuracyScore = doc.GetValue<float>("accuracyScore")
                });
            }
            return sessions;
        }
    }
}
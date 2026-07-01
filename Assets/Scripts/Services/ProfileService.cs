using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using App.Data.Models;
using UserProfile = App.Data.Models.UserProfile;
using System;

namespace App.Services
{
    public class ProfileService
    {
        private readonly FirebaseFirestore _db;
        private readonly FirebaseAuth _auth;

        public ProfileService()
        {
            _db = FirebaseFirestore.DefaultInstance;
            _auth = FirebaseAuth.DefaultInstance;
        }

        // Call this after successful registration
        public async Task CreateProfileAsync(Data.Models.UserProfile profile)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    { "userId",                  profile.userId },
                    { "subjectId",               profile.subjectId ?? "" },
                    { "firstName",               profile.firstName },
                    { "lastName",                profile.lastName },
                    { "email",                   profile.email },
                    { "registrationDate",        profile.registrationDate.ToString("o") },
                    { "totalSessionsCompleted",  profile.totalSessionsCompleted },
                    { "affectedSide",            profile.affectedSide.ToString() },
                    { "surgeryDate",             profile.surgeryDate }
                };

                await _db.Collection("users")
                         .Document(profile.userId)
                         .SetAsync(data);

                Debug.Log($"[ProfileService] Profile created for {profile.email}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProfileService] Failed to create profile: {e.Message}");
                throw;
            }
        }

        // Call this on login to load the user's profile
        public async Task<UserProfile> LoadProfileAsync(string userId)
        {
            try
            {
                var snapshot = await _db.Collection("users")
                                        .Document(userId)
                                        .GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    Debug.LogWarning($"[ProfileService] No profile found for {userId}");
                    return null;
                }

                string loadedSurgeryDate = "Not Specified";
                if (snapshot.TryGetValue<string>("surgeryDate", out string sDate))
                {
                    loadedSurgeryDate = sDate;
                }

                AffectedSide loadedAffectedSide = AffectedSide.Unknown;
                if (snapshot.TryGetValue<string>("affectedSide", out string aSideStr))
                {
                    if (!Enum.TryParse(aSideStr, true, out loadedAffectedSide))
                    {
                        // Fallback: se a base de dados antiga tiver "Esquerdo" ou "Indefinido" em português
                        if (aSideStr.ToLower().Contains("esquerd")) loadedAffectedSide = AffectedSide.Left;
                        else if (aSideStr.ToLower().Contains("direit")) loadedAffectedSide = AffectedSide.Right;
                        else if (aSideStr.ToLower().Contains("bilateral")) loadedAffectedSide = AffectedSide.Bilateral;
                        else loadedAffectedSide = AffectedSide.Unknown;
                    }
                }

                var profile = new UserProfile
                {
                    userId = snapshot.GetValue<string>("userId"),
                    subjectId = snapshot.ContainsField("subjectId") ? snapshot.GetValue<string>("subjectId") : "",
                    firstName = snapshot.GetValue<string>("firstName"),
                    lastName = snapshot.GetValue<string>("lastName"),
                    email = snapshot.GetValue<string>("email"),
                    affectedSide = loadedAffectedSide,
                    surgeryDate = loadedSurgeryDate,
                    totalSessionsCompleted = snapshot.GetValue<int>("totalSessionsCompleted"),
                    registrationDate = DateTime.Parse(snapshot.GetValue<string>("registrationDate"))
                };

                return profile;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProfileService] Failed to load profile: {e.Message}");
                throw;
            }
        }

        // Call this after each completed session
        public async Task IncrementSessionCountAsync(string userId)
        {
            try
            {
                await _db.Collection("users")
                         .Document(userId)
                         .UpdateAsync("totalSessionsCompleted",
                             FieldValue.Increment(1));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProfileService] Failed to increment sessions: {e.Message}");
                throw;
            }
        }

        // Call this when user updates their affected side
        public async Task UpdateAffectedSideAsync(string userId, string affectedSide)
        {
            try
            {
                await _db.Collection("users")
                         .Document(userId)
                         .UpdateAsync("affectedSide", affectedSide);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProfileService] Failed to update affected side: {e.Message}");
                throw;
            }
        }
    }
}
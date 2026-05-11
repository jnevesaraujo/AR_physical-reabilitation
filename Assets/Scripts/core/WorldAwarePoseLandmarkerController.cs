using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using UnityEngine;

namespace App.Core
{
    public class WorldAwarePoseLandmarkerController : PoseLandmarkerResultAnnotationController
    {
        public new void DrawLater(PoseLandmarkerResult target)
        {
            Debug.Log("[WorldAware] DrawLater called");
            
            if (target.poseWorldLandmarks != null && target.poseWorldLandmarks.Count > 0)
            {
                WorldLandmarkProvider.UpdateLandmarks(target.poseWorldLandmarks[0]);
            }
            base.DrawLater(target);
        }

        public new void DrawNow(PoseLandmarkerResult target)
        {
            if (target.poseWorldLandmarks != null && target.poseWorldLandmarks.Count > 0)
            {
                WorldLandmarkProvider.UpdateLandmarks(target.poseWorldLandmarks[0]);
            }
            base.DrawNow(target);
        }
    }
}
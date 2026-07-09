using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class ExerciseMenuView : MonoBehaviour
    {
        public event Action OnNeckRotationSelected;
        public event Action OnHandGripSelected;
        public event Action OnShoulderSlideSelected;
        public event Action OnElbowFlexionSelected;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Q<VisualElement>("btn-neckRotation")?.RegisterCallback<ClickEvent>(_ => OnNeckRotationSelected?.Invoke());
            root.Q<VisualElement>("btn-handGrip")?.RegisterCallback<ClickEvent>(_ => OnHandGripSelected?.Invoke());
            root.Q<VisualElement>("btn-shoulderSlide")?.RegisterCallback<ClickEvent>(_ => OnShoulderSlideSelected?.Invoke());
            root.Q<VisualElement>("btn-elbowFlexion")?.RegisterCallback<ClickEvent>(_ => OnElbowFlexionSelected?.Invoke());
        }
    }
}
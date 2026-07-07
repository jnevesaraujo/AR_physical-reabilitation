using UnityEngine.UIElements;

public static class GeneralEffects
{
    // Extension method to be called on any VisualElement.
    public static void AddTouchFeedback(this VisualElement el)
    {
        el.RegisterCallback<PointerDownEvent>(_ => el.style.opacity = 0.5f);
        el.RegisterCallback<PointerUpEvent>(_ => el.style.opacity = 1.0f);
        el.RegisterCallback<PointerLeaveEvent>(_ => el.style.opacity = 1.0f);
    }
}
    
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class RehabAppEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/RehabAppEditor")]
    public static void ShowExample()
    {
        RehabAppEditor wnd = GetWindow<RehabAppEditor>();
        wnd.titleContent = new GUIContent("RehabAppEditor");
    }

    private int m_ClickCount = 0;
    private const string m_ButtonPrefix = "button";

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("These controls were created using C# code.");
        root.Add(label);

        Button button = new Button
        {
            name = "button3",
            text = "This is button3."
        };

        Toggle toggle = new Toggle
        {
            name = "toggle3",
            label = "Number?"
        };

        root.Add(button);
        root.Add(toggle);

        // Instantiate UXML created automatically which is set as the default VisualTreeAsset.
        root.Add(m_VisualTreeAsset.Instantiate());

        // Import UXML created manually.
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/RehabAppEditor_UXML.uxml");
        VisualElement labelFromUXML2 = visualTree.Instantiate();
        root.Add(labelFromUXML2);

        SetupButtonHandler();
    }

    //Functions as the event handlers for your button click and number counts
    private void SetupButtonHandler()
    {
        VisualElement root = rootVisualElement;

        var buttons = root.Query<Button>();
        buttons.ForEach(RegisterHandler);
    }

    private void RegisterHandler(Button button)
    {
        button.RegisterCallback<ClickEvent>(PrintClickMessage);
    }

    private void PrintClickMessage(ClickEvent evt)
    {
        VisualElement root = rootVisualElement;

        ++m_ClickCount;

        //Because of the names we gave the buttons and toggles, we can use the
        //button name to find the toggle name.
        Button button = evt.currentTarget as Button;
        string buttonNumber = button.name.Substring(m_ButtonPrefix.Length);
        string toggleName = "toggle" + buttonNumber;
        Toggle toggle = root.Q<Toggle>(toggleName);

        Debug.Log("Button was clicked!" +
            (toggle.value ? " Count: " + m_ClickCount : ""));
    }
}

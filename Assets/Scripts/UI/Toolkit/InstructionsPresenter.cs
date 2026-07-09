using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using App.Core;

public class InstructionsPresenter : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Label _lblName;
    private Label _lblDescription;
    private VisualElement _imgTutorial;
    private Button _btnStart;

    private void OnEnable()
    {
        Debug.Log("[InstructionsPresenter] OnEnable called.");
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        var root = _uiDocument.rootVisualElement;

        _btnStart = root.Q<Button>("btn_start");
        _lblName = root.Q<Label>("lbl_exerciseName");
        _lblDescription = root.Q<Label>("lbl_description");
        _imgTutorial = root.Q<VisualElement>("img_tutorial");

        if (_btnStart != null)
        {
            _btnStart.clicked -= OnStartClicked;
            _btnStart.clicked += OnStartClicked;
        }

        PopulateFromSession();
    }

    public void PopulateFromSession()
    {
        var exercise = SessionContext.CurrentExercise;
        if (exercise == null) return;

        _lblName.text = exercise.exerciseName;
        _lblDescription.text = exercise.description;
        _lblDescription.style.whiteSpace = WhiteSpace.Normal;

        if (exercise.tutorialIcon != null)
        {
            _imgTutorial.style.backgroundImage = new StyleBackground(exercise.tutorialIcon);
        }
    }

    private void OnStartClicked()
    {
        string targetScene = SessionContext.TargetARScene;

        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        } else
        {
            Debug.LogError("[InstructionsPresenter] Cena de destino não encontrada.");
        }
    }
}
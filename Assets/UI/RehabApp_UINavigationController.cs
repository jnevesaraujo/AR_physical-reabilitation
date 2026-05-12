using UnityEngine;
using UnityEngine.UIElements;

public class RehabApp_UINavigationController : MonoBehaviour
{
    private VisualElement _screenMenu;
    private VisualElement _screenExercise;

    private Button _btnIniciarExercicio;
    private Button _btnVoltarMenu;

    private void OnEnable()
    {
        // 1. Obter o elemento raiz do UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 2. Extrair referências dos painéis
        _screenMenu = root.Q<VisualElement>("Ecra_MenuPrincipal");
        _screenExercise = root.Q<VisualElement>("Ecra_Exercicio");

        // 3. Extrair referências dos botões
        _btnIniciarExercicio = root.Q<Button>("Btn_IniciarExercicio");
        _btnVoltarMenu = root.Q<Button>("Btn_VoltarMenu");

        // 4. Subscrever eventos de clique
        if (_btnIniciarExercicio != null) 
            _btnIniciarExercicio.clicked += NavegarParaExercicio;
            
        if (_btnVoltarMenu != null) 
            _btnVoltarMenu.clicked += NavegarParaMenu;
    }

    private void OnDisable()
    {
        // 5. Remover subscrição de eventos ao desativar o componente
        if (_btnIniciarExercicio != null) 
            _btnIniciarExercicio.clicked -= NavegarParaExercicio;
            
        if (_btnVoltarMenu != null) 
            _btnVoltarMenu.clicked -= NavegarParaMenu;
    }

    private void NavegarParaExercicio()
    {
        _screenMenu.style.display = DisplayStyle.None;
        _screenExercise.style.display = DisplayStyle.Flex;
    }

    private void NavegarParaMenu()
    {
        _screenExercise.style.display = DisplayStyle.None;
        _screenMenu.style.display = DisplayStyle.Flex;
    }
}
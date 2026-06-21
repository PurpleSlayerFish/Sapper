using UnityEngine;
using UnityEngine.UI;

// Вью с собственной кнопкой и ссылкой на перехватываемую кнопку (например, кнопку Close окна)
public class ReferenceButtonInterceptorView : BaseUiElementView
{
    [SerializeField] private Button _selfButton;
    [SerializeField] private Button _targetButton;

    public Button SelfButton => _selfButton;
    public Button TargetButton => _targetButton;
}

public class ReferenceButtonInterceptorController : BaseUiElementController<ReferenceButtonInterceptorView>
{
    public ReferenceButtonInterceptorController(ReferenceButtonInterceptorView view) : base(view)
    {
    }

    protected override void OnInitialize()
    {
        if (View.SelfButton != null)
            View.SelfButton.onClick.AddListener(HandleSelfButtonClicked);
    }

    private void HandleSelfButtonClicked()
    {
        if (View.TargetButton != null && View.TargetButton.interactable)
            View.TargetButton.onClick.Invoke();
    }

    protected override void OnDispose()
    {
        if (View.SelfButton != null)
            View.SelfButton.onClick.RemoveListener(HandleSelfButtonClicked);
    }
}
using UI;

// Вью с собственной кнопкой и ссылкой на перехватываемую кнопку (например, кнопку Close окна)
public class ReferenceButtonInterceptorView : BaseUiElementView
{
    public UiButtonView SelfButton;
    public UiButtonView TargetButton;
}

public class ReferenceButtonInterceptorController : BaseUiElementController<ReferenceButtonInterceptorView>
{
    protected override void OnInit()
    {
        if (View.SelfButton != null)
            View.SelfButton.Subscribe(HandleSelfButtonClicked);
    }

    private void HandleSelfButtonClicked()
    {
        if (View.TargetButton && View.TargetButton.Interactable)
            View.TargetButton.Invoke();
    }
}
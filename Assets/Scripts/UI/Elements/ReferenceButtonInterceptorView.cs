using UI;

// Вью с собственной кнопкой и ссылкой на перехватываемую кнопку (например, кнопку Close окна)
public class ReferenceButtonInterceptorView : BaseUiElementView
{
    public UiButtonView SelfButton;
    public UiButtonView TargetButton;
}

public class ReferenceButtonInterceptorUiElementController : BaseUiElementController<ReferenceButtonInterceptorView>
{
    public override void OnAfterInit()
    {
        if (View.SelfButton != null)
            Disposables.Add(View.SelfButton.Subscribe(HandleSelfButtonClicked));
    }

    private void HandleSelfButtonClicked()
    {
        if (View.TargetButton && View.TargetButton.Interactable)
            View.TargetButton.Invoke();
    }
}
using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class UiButtonView : BaseUiElementView
    {
        [RequiredMember, SerializeField] private Button _button;
        
        public bool Interactable => _button.enabled && _button.interactable;

        public IDisposable Subscribe(Action action)
        {
            _button.onClick.AddListener(action.Invoke);
            return this;
        }
        
        public IDisposable Unsubscribe(Action action)
        {
            _button.onClick.RemoveListener(action.Invoke);
            return this;
        }
        
        public void Invoke () => _button.onClick.Invoke();

        public override void Dispose()
        {
            _button.onClick.RemoveAllListeners();
            base.Dispose();
        }
    }
}
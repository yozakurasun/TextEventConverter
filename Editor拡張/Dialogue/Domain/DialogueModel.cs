using R3;
using System;
using UnityEngine.InputSystem;

public class DialogueModel : IDisposable
{
    private ReactiveProperty<bool> _input = new ReactiveProperty<bool>(false);
    public ReadOnlyReactiveProperty<bool> Input => _input;

    private InputAction _textInputEventClick;
    private InputAction _textInputEventSpace;

    public DialogueModel()
    {
        //マウスクリックで入力検知するためのInputActionを作成
        _textInputEventClick = new InputAction("TextInput", binding: "<Mouse>/leftButton");
        _textInputEventSpace = new InputAction("TextInput", binding: "<Keyboard>/space");
        _textInputEventClick.started += OnInput;
        _textInputEventSpace.started += OnInput;
        _textInputEventClick.Enable();
        _textInputEventSpace.Enable();
    }

    public void SetInput(bool value)
    {
        _input.Value = value;
    }

    public void OnInput(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            SetInput(true);
        }
    }

    public void Dispose()
    {
        if(_textInputEventClick == null) return;
        _textInputEventClick.started -= OnInput;
        _textInputEventClick.Dispose();
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTest : MonoBehaviour
{
    [SerializeField] private DialoguePresenter _dialoguePresenter;
    [SerializeField] private DialogueType _testDialogueType;

    private InputAction _textEventTest;

    private void Start()
    {
        //_dialoguePresenter.SetOnCharacterMovementRestriction(OnCharacterMovementRestriction);
        _textEventTest = new InputAction("TextEventTest", binding: "<Keyboard>/enter");
        _textEventTest.started += OnTextEventTest;
        _textEventTest.Enable();
    }

    private void OnTextEventTest(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _dialoguePresenter.SetTextEvent(_testDialogueType);
        }
    }
}
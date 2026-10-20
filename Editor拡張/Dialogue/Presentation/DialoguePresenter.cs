using UnityEngine;
using R3;
using System;

public class DialoguePresenter : MonoBehaviour
{
    [SerializeField] private DialogueView _dialogueView;
    private DialogueModel _dialogueModel;

    private Action<bool> _onCharacterMovementRestriction;

    private void Awake()
    {
        _dialogueModel = new DialogueModel();
        _dialogueView.Initialize(_onCharacterMovementRestriction);
    }

    private void Start()
    {
        _dialogueModel.Input.Subscribe(input =>
        {
            if (input == false) return;
            if (_dialogueView.IsViewingDialogue == false)
            {//ダイアログ表示中でないなら入力を無効化
                _dialogueModel.SetInput(false);
                return;
            }

            if(_dialogueView.WaitInput)
            {//入力待機状態なら次の文章へ
                _dialogueView.OnNextSentence();
                Debug.Log("次の文章へ");
            }
            else
            {//入力待機状態でないならタイプライターをキャンセル
                _dialogueView.OnCancelTypewrite();
                Debug.Log("タイプライターをキャンセル");
            }

            _dialogueModel.SetInput(false);
        });
    }

    public void SetTextEvent(DialogueType dialogueType)
    {
        _dialogueView.ViewDialogueEvent(dialogueType);
    }

    public void SetOnCharacterMovementRestriction(Action<bool> onCharacterMovementRestriction)
    {
        _onCharacterMovementRestriction += onCharacterMovementRestriction;
    }

    public void OnDestroy()
    {
        _dialogueModel.Dispose();
    }
}

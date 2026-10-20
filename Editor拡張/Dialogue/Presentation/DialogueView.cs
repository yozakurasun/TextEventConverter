using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using System.Threading;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private CanvasGroup _dialogueCanvasGroup;
    [SerializeField] private float _fadeDuration = 0.5f;

    [SerializeField] private TextMeshProUGUI _speakerNameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Image _speakerImageLeft;
    [SerializeField] private Image _speakerImageRight;
    private bool _isViewSpeakerLeft = false;
    private bool _isViewSpeakerRight = false;

    [SerializeField] private Image _waitInputAllow;
    [SerializeField] private float _waitInputAnimDuration = 0.5f;
    [SerializeField] private float _waitInputAnimPosOffset = 20f;
    private Vector2 _waitInputAnimStartPos;

    [SerializeField] private DialogueTextDatas _dialogueTextDatas;
    [SerializeField] private SpeakerDatasInfo _speakerDatasInfo;
    private DialogueDatas _currentDialogDatas;

    private bool _isViewingDialogue = false; //ダイアログ表示中かどうか
    public bool IsViewingDialogue => _isViewingDialogue;

    private bool _ｗaitInput;//入力待機状態かどうか
    public bool WaitInput => _ｗaitInput;

    private CancellationTokenSource _typewriteCts = new CancellationTokenSource();
    private CancellationTokenSource _waitAnimCts = new CancellationTokenSource();

    /// <summary>
    /// PlayerやEnemyの移動制御を行うためのコールバック
    /// true: 移動制御を有効化する, false: 移動制御を無効化する
    /// </summary>
    private Action<bool> _onCharacterMovementRestriction;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="onCharacterMovementRestriction">PlayerやEnemyの移動制御を行うためのコールバック</param>
    public void Initialize(Action<bool> onCharacterMovementRestriction)
    {
        _onCharacterMovementRestriction = onCharacterMovementRestriction;
        _dialogueCanvasGroup.alpha = 0f; // 初期状態では非表示
        _speakerImageLeft.color = new Color(1, 1, 1, 0);
        _speakerImageRight.color = new Color(1, 1, 1, 0);
        _waitInputAnimStartPos = _waitInputAllow.rectTransform.localPosition; //入力待機アニメーションの初期位置を保存
        _waitInputAllow.gameObject.SetActive(false); //入力待機アニメーションを非表示
    }

    /// <summary>
    /// 指定のシナリオテキストをリスト順に表示する
    /// </summary>
    /// <param name="dialogueType">シナリオの種類</param>
    public async void ViewDialogueEvent(DialogueType dialogueType)
    {
        if(_isViewingDialogue)
        {
            Debug.LogWarning("ダイアログ表示中に再度表示しようとしました");
            return;
        }
        _currentDialogDatas = _dialogueTextDatas.DialogueDatasList.FirstOrDefault(x => x.DialogueType == dialogueType);
        if (_currentDialogDatas == null)
        {
            Debug.LogWarning($"指定タイプのテキストデータが見つかりませんでした　=>　{dialogueType}");
            return;
        }

        _onCharacterMovementRestriction?.Invoke(true);//移動制御を有効化
        await ViewDialogue();//ダイアログ表示

        foreach (var textData in _currentDialogDatas.TextDatas)
        {//リスト順に一文ずつ表示
            await SetTextDialogue(textData.SpeakerName, textData.Text, textData.TypewriterDuration, textData.SpeakerExpressionType, textData.IsPosLeft);
        }

        await HideDialogue();//ダイアログ非表示
        _onCharacterMovementRestriction?.Invoke(false);//移動制御を無効化
    }

    /// <summary>
    /// タイプライター表示をスキップ
    /// </summary>
    public void OnCancelTypewrite()
    {
        _typewriteCts.Cancel(); //タイプライター表示をキャンセル
    }

    /// <summary>
    /// 次の文章へ
    /// </summary>
    public void OnNextSentence()
    {
        _ｗaitInput = false; //入力待機状態を解除
        _waitInputAllow.gameObject.SetActive(false); //入力待機アニメーションを非表示
        _waitAnimCts.Cancel(); //入力待機アニメーションをキャンセル
    }

    /// <summary>
    /// 入力待機状態に設定
    /// 入力待機の矢印を表示してアニメーションを開始する
    /// </summary>
    private void SetWaitInput()
    {
        _waitInputAllow.rectTransform.localPosition = _waitInputAnimStartPos; //入力待機アニメーションの初期位置に戻す
        _waitInputAllow.gameObject.SetActive(true); //入力待機アニメーションを表示
        _waitAnimCts = new CancellationTokenSource(); //キャンセルトークンの初期化
        Vector2 targetPosition = new Vector2(_waitInputAnimStartPos.x, _waitInputAnimStartPos.y - _waitInputAnimPosOffset); //アニメーションの目標位置を指定
        MoveRectAnim(_waitInputAllow.rectTransform, targetPosition, _waitInputAnimDuration, Ease.InOutSine, -1, LoopType.Yoyo, _waitAnimCts.Token).Forget(); //入力待機アニメーション開始
        _ｗaitInput = true; //入力待機状態に設定
    }

    #region 表示非表示処理
    /// <summary>
    /// テキスト一文を一文字ずつ表示する
    /// </summary>
    /// <param name="dialogueText">表示するテキスト</param>
    /// <param name="typewriterDuration">一文字ごとの表示時間</param>
    /// <param name="speakerImage">話者の画像</param>
    /// <returns></returns>
    private async UniTask SetTextDialogue(string speakerName, string dialogueText, float typewriterDuration, SpeakerExpressionType speakerExpressionType, bool isPosLeft)
    {
        _typewriteCts = new CancellationTokenSource(); //キャンセルトークンの初期化
        SetSpeakerImage(_speakerDatasInfo.GetExpressionSprite(speakerName, speakerExpressionType), isPosLeft); //話者の画像を設定        
        _speakerNameText.text = _speakerDatasInfo.GetSpeakerName(speakerName); //話者の名前を設定

        _dialogueText.text = dialogueText;
        _dialogueText.maxVisibleCharacters = 0; //テキストを非表示にする
        _dialogueText.ForceMeshUpdate(); //テキストのメッシュを更新

        for (int i = 0; i <= dialogueText.Length; i++)
        {
            _dialogueText.maxVisibleCharacters = i;
            if(_typewriteCts.Token.IsCancellationRequested)
            {
                _dialogueText.maxVisibleCharacters = dialogueText.Length; //キャンセルされた場合は全て表示
                break;
            }
            await UniTask.WaitForSeconds(typewriterDuration);
        }
        _dialogueText.maxVisibleCharacters = dialogueText.Length; //全て表示
        SetWaitInput(); //入力待機状態に設定
        await UniTask.WaitUntil(() => !_ｗaitInput); //入力待機
    }

    /// <summary>
    /// 表示処理
    /// </summary>
    /// <returns></returns>
    private async UniTask ViewDialogue()
    {
        _dialogueText.text = ""; // テキストを空にする
        SetSpeakerImage(_speakerDatasInfo.GetExpressionSprite(_currentDialogDatas.TextDatas[0].SpeakerName, _currentDialogDatas.TextDatas[0].SpeakerExpressionType), _currentDialogDatas.TextDatas[0].IsPosLeft); // 最初の話者画像を設定
        _speakerNameText.text = _speakerDatasInfo.GetSpeakerName(_currentDialogDatas.TextDatas[0].SpeakerName); //最初の話者の名前を設定

        _dialogueCanvasGroup.interactable = true;
        _dialogueCanvasGroup.blocksRaycasts = true;
        await FadeCanvasGroup(_dialogueCanvasGroup, 1f, _fadeDuration);
        _isViewingDialogue = true;
    }

    /// <summary>
    /// 非表示処理
    /// </summary>
    /// <returns></returns>
    private async UniTask HideDialogue()
    {
        _isViewingDialogue = false;
        _dialogueCanvasGroup.interactable = false;
        _dialogueCanvasGroup.blocksRaycasts = false;
        await FadeCanvasGroup(_dialogueCanvasGroup, 0f, _fadeDuration);
        _speakerImageLeft.color = new Color(1, 1, 1, 0);
        _speakerImageRight.color = new Color(1, 1, 1, 0);
        _isViewSpeakerLeft = false;
        _isViewSpeakerRight = false;
    }

    /// <summary>
    /// 話者の画像を設定する関数
    /// キャラクターは左右に配置され、話している方が明るく、そうでない方が暗くなる
    /// </summary>
    /// <param name="speakerSprite">話者の画像</param>
    /// <param name="isLeft">キャラクターの位置</param>
    private void SetSpeakerImage(Sprite speakerSprite, bool isLeft)
    {
        if (isLeft)
        {//左のキャラクターが話者
            if (!_isViewSpeakerLeft)
            {//話者がまだ表示されていない
                _isViewSpeakerLeft = true;
            }
            _speakerImageLeft.color = new Color(1, 1, 1, 1);
            _speakerImageLeft.sprite = speakerSprite;

            if (_isViewSpeakerRight)
            {//右の話者が表示されていたら暗くする
                _speakerImageRight.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }
        else
        {//右のキャラクターが話者
            if (!_isViewSpeakerRight)
            {//話者がまだ表示されていない
                _isViewSpeakerRight = true;
            }
            _speakerImageRight.color = new Color(1, 1, 1, 1);
            _speakerImageRight.sprite = speakerSprite;

            if (_isViewSpeakerLeft)
            {//左の話者が表示されていたら暗くする
                _speakerImageLeft.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }
    }

    /// <summary>
    /// フェード関数
    /// </summary>
    /// <param name="canvasGroup">フェードさせるCanvasGroup</param>
    /// <param name="targetAlpha">目標のアルファ値</param>
    /// <param name="duration">フェードにかかる時間</param>
    /// <returns></returns>
    private async UniTask FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        await LMotion.Create(canvasGroup.alpha, targetAlpha, duration)
            .WithEase(Ease.Linear)
            .Bind(alpha =>
            {
                canvasGroup.alpha = alpha;
            }).ToUniTask();
        canvasGroup.alpha = targetAlpha;
    }

    private async UniTask MoveRectAnim(RectTransform targetRect, Vector3 targetPosition, float duration, Ease ease, int loopCount, LoopType loopType, CancellationToken token)
    {
        try
        {
            await LMotion.Create(targetRect.localPosition, targetPosition, duration)
                .WithLoops(loopCount, loopType)
                .WithEase(ease)
                .Bind(position =>
                {
                    targetRect.localPosition = position;
                }).ToUniTask(token);
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合の処理（必要に応じて追加）
        }

    }
    #endregion

    private void OnDestroy()
    {
        if(_typewriteCts == null) return;
        _typewriteCts.Cancel();
        _typewriteCts = null;
    }
}

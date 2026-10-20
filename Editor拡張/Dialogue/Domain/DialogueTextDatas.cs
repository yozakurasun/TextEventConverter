using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueTextDatas", menuName = "ScriptableObjects/DialogueTextDatas", order = 1)]
public class DialogueTextDatas : ScriptableObject
{
    [SerializeField] public List<DialogueDatas> DialogueDatasList = new List<DialogueDatas>();
}

/// <summary>
/// 場面ごとのテキストデータリスト
/// </summary>
[Serializable]
public class DialogueDatas
{
    public DialogueType DialogueType;
    public List<TextData> TextDatas;
}

/// <summary>
/// 表示するテキストのデータ一塊
/// Text　=>　一文
/// TypewriterDuration　=>　一文字ごとの表示時間
/// SpeakerImage　=>　話者の画像
/// </summary>
[Serializable]
public class TextData
{
    public string SpeakerName;
    [TextArea]
    public string Text;
    public float TypewriterDuration = 0.05f;
    public bool IsPosLeft = true;
    public SpeakerExpressionType SpeakerExpressionType;
}

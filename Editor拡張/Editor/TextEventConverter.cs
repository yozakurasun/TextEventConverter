using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TextEventConverter : EditorWindow
{
    private TextAsset _textJsonAsset;
    private DialogueTextDatas _dialogueDatas;
    private List<DialogueDatas> _dialogueDatasList = new List<DialogueDatas>();

    [MenuItem("Tools/TextEventConverter")]
    static void Init()
    {
        TextEventConverter window = (TextEventConverter)EditorWindow.GetWindow(typeof(TextEventConverter));
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("TextEventConverter", EditorStyles.boldLabel);

        GUILayout.Space(10f);

        JsonFileField();

        GUILayout.Space(10f);

        DialogueDatasField();

        GUILayout.Space(10f);

        if (GUILayout.Button("テキストデータのJsonをスクリプタブルオブジェクトに変換"))
        {
            ConvertTextEvents();
        }
    }

    private void JsonFileField()
    {
        GUILayout.Label("テキストデータのJsonファイル↓", EditorStyles.label);
        _textJsonAsset = (TextAsset)EditorGUILayout.ObjectField(_textJsonAsset, typeof(TextAsset), false);
    }

    private void DialogueDatasField()
    {
        GUILayout.Label("変換先スクリプタブルオブジェクト↓", EditorStyles.label);
        _dialogueDatas = (DialogueTextDatas)EditorGUILayout.ObjectField(_dialogueDatas, typeof(DialogueTextDatas), false);
    }

    /// <summary>
    /// 変換ボタンを押したときの関数
    /// </summary>
    private void ConvertTextEvents()
    {
        if (_textJsonAsset == null || _dialogueDatas == null)
        {
            Debug.LogError("Jsonファイルまたはスクリプタブルオブジェクトが設定されていません。");
            return;
        }

        // Jsonファイルの内容をDialogueDatasListに変換
        MakeData();
    }

    /// <summary>
    /// Jsonデータをスクリプタブルオブジェクトに変換
    /// </summary>
    private void MakeData()
    {
        try
        {
            _dialogueDatas.DialogueDatasList.Clear();

            _dialogueDatasList.Clear();
            _dialogueDatasList = new List<DialogueDatas>();

            // JSONのキーをC#で扱える名前に変更する。
            string json = _textJsonAsset.text;

            // JSONをクラスに変換する。
            JsonRoot root = JsonUtility.FromJson<JsonRoot>(json);

            foreach (DialogueType dialogueType in Enum.GetValues(typeof(DialogueType)))
            {//EnumType毎に区切る
                if (dialogueType == DialogueType.None) continue;
                List<JsonTextEvent> rows = root.TextEventData.Where(x => GetParseDialogueType(x.TextEventType) == dialogueType).ToList();

                List<TextData> datas = new List<TextData>();
                foreach (JsonTextEvent row in rows)
                {//テキストデータを設定
                    TextData textData = new TextData();
                    textData.SpeakerName = row.Name;
                    textData.Text = row.TextJP;
                    textData.TypewriterDuration = GetParseTypewriterDuration(row.TypewriterDuration);
                    textData.IsPosLeft = row.Side == "Right" ? false : true;
                    datas.Add(textData);
                }

                if (datas.Count <= 0) continue; //指定タイプのテキストデータがなかったらデータを作らない

                DialogueDatas dialogueDatas = new DialogueDatas();
                dialogueDatas.DialogueType = dialogueType;
                dialogueDatas.TextDatas = datas;
                _dialogueDatasList.Add(dialogueDatas);
            }
            _dialogueDatas.DialogueDatasList = _dialogueDatasList;
        }
        catch
        {
            Debug.LogError("処理が途中でキャンセルされました。");
        }
    }

    #region Parse群
    private DialogueType GetParseDialogueType(string dialogueTypeSt)
    {
        if (dialogueTypeSt == string.Empty)
        {
            Debug.LogWarning($"TextEventTypeに何も書かれていません。");
            return DialogueType.None;
        }

        if (Enum.TryParse(dialogueTypeSt, out DialogueType dialogueType))
        {
            return dialogueType;
        }
        else
        {
            Debug.LogWarning($"指定のEnumが見つかりませんでした。 => {dialogueType}");
            return DialogueType.None;
        }
    }

    private float GetParseTypewriterDuration(string typewriterDurationSt)
    {
        if (typewriterDurationSt == string.Empty)
        {
            Debug.LogWarning($"TypeWriterDurationに何も書かれていません。");
            return 0.05f;
        }

        if (float.TryParse(typewriterDurationSt, out float typewriterDuration))
        {
            return typewriterDuration;
        }
        else
        {
            Debug.LogWarning($"文字列の形式が間違っています。。 => {typewriterDuration}");
            return 0.05f;
        }
    }
    #endregion

    // JSON全体を受け取るクラス。
    [Serializable]
    private class JsonRoot
    {
        public List<JsonTextEvent> TextEventData;
    }

    // JSONの各要素を受け取るクラス。
    [Serializable]
    private class JsonTextEvent
    {
        public string TextEventType;
        public string Name;
        public string Side;
        public string TextJP;
        public string TextEN;
        public string Order;
        public string TypewriterDuration;
        public string SpeakerExpressionType;
        public string Memo;
    }
}

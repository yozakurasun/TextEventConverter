using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeakerDatasInfo", menuName = "ScriptableObjects/SpeakerDatasInfo", order = 2)]
public class SpeakerDatasInfo : ScriptableObject
{
    [SerializeField] public List<SpeakerData> SpeakerDatas = new List<SpeakerData>();

    public string GetSpeakerName(string speakerName, bool isJP = true)
    {
        var speakerData = SpeakerDatas.FirstOrDefault(x => x.SpeakerName == speakerName);
        if (speakerData == null)
        {
            Debug.LogWarning($"指定のキャラクターデータが見つかりませんでした。");
            return null;
        }

        if (isJP) return speakerData.NameData.NameJP;
        else return speakerData.NameData.NameEN;
    }

    public Sprite GetExpressionSprite(string speakerName, SpeakerExpressionType expressionType)
    {
        var speakerData = SpeakerDatas.FirstOrDefault(x => x.SpeakerName == speakerName);
        if (speakerData == null)
        {
            Debug.LogWarning($"指定のキャラクターデータが見つかりませんでした。");
            return null;
        }

        var expressionData = speakerData.ExpressionDatas.FirstOrDefault(x => x.SpeakerExpressionType == expressionType);
        if (expressionData == null || expressionData.Sprite == null)
        {
            Debug.LogWarning($"指定のキャラクターの表情差分画像が見つかりませんでした。");
            return null;
        }

        return expressionData.Sprite;
    }
}

[Serializable]
public class SpeakerData
{
    public string SpeakerName;
    public NameData NameData;
    public List<ExpressionData> ExpressionDatas;

}

[Serializable]
public class NameData
{
    public string NameJP;
    public string NameEN;
}

[Serializable]
public class ExpressionData
{
    public SpeakerExpressionType SpeakerExpressionType;
    public Sprite Sprite;
}

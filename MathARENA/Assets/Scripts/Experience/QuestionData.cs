using UnityEngine;

[System.Serializable]
public class QuestionData
{
    [TextArea]
    public string questionText;

    public string[] choices;   // 4지선다용 (주관식이면 비워두기)
    public int correctIndex;   // 0~3

    public ExperienceDifficulty difficulty; // VeryEasy, Hard 등
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceDifficultyBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private TMP_Text difficultyText;

    [SerializeField]
    private Image difficultyIcon;

    [Header("Difficulty Icons")]
    [SerializeField]
    private Sprite iconVeryEasy;

    [SerializeField]
    private Sprite iconEasy;

    [SerializeField]
    private Sprite iconHard;

    [SerializeField]
    private Sprite iconVeryHard;

    [SerializeField]
    private Sprite iconTough;

    [SerializeField]
    private Sprite iconVeryTough;

    // [중요] 타이머 관련 변수와 Update 문을 모두 삭제했습니다.

    public void ApplyDifficulty(ExperienceDifficulty difficulty)
    {
        if (difficultyText != null)
            difficultyText.text = difficulty.ToString().Replace("Very", "Very ");

        if (difficultyIcon != null)
            difficultyIcon.sprite = GetDifficultySprite(difficulty);

        Debug.Log($"[난이도] {difficulty} 아이콘 적용");
    }

    private Sprite GetDifficultySprite(ExperienceDifficulty diff)
    {
        return diff switch
        {
            ExperienceDifficulty.VeryEasy => iconVeryEasy,
            ExperienceDifficulty.Easy => iconEasy,
            ExperienceDifficulty.Hard => iconHard,
            ExperienceDifficulty.VeryHard => iconVeryHard,
            ExperienceDifficulty.Tough => iconTough,
            ExperienceDifficulty.VeryTough => iconVeryTough,
            _ => iconVeryEasy,
        };
    }
}

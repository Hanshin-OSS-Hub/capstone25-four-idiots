using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceDifficultyBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text difficultyText; // 난이도 텍스트 (예: "Tough")
    [SerializeField] private Image difficultyIcon;    // 표정 NPC 이미지

    [Header("Difficulty Icons (1-6)")]
    [SerializeField] private Sprite iconVeryEasy;  // 1. VERY EASY.jpg
    [SerializeField] private Sprite iconEasy;      // 2. EASY.jpg
    [SerializeField] private Sprite iconHard;      // 3. HARD.jpg
    [SerializeField] private Sprite iconVeryHard;  // 4. VERY HARD.jpg
    [SerializeField] private Sprite iconTough;     // 5. TOUGH.jpg
    [SerializeField] private Sprite iconVeryTough; // 6. VERY TOUGH.jpg

    private void Start()
    {
        // 현재 세션의 난이도를 기반으로 초기화
        ApplyDifficulty(ExperienceSession.CurrentDifficulty);
    }

    /// <summary>
    /// 난이도에 따라 텍스트와 NPC 아이콘을 변경합니다.
    /// </summary>
    public void ApplyDifficulty(ExperienceDifficulty difficulty)
    {
        // 1. 텍스트 업데이트
        if (difficultyText != null)
        {
            difficultyText.text = GetDifficultyString(difficulty);
        }

        // 2. 아이콘(표정 NPC) 업데이트
        if (difficultyIcon != null)
        {
            Sprite targetSprite = GetDifficultySprite(difficulty);
            if (targetSprite != null)
            {
                difficultyIcon.sprite = targetSprite;
                // 이미지 본연의 색상을 보여주기 위해 Color를 화이트로 초기화합니다.
                difficultyIcon.color = Color.white; 
            }
        }
    }

    // 난이도별 출력 문자열 반환 (EX-04-1 대응)
    private string GetDifficultyString(ExperienceDifficulty difficulty)
    {
        return difficulty switch
        {
            ExperienceDifficulty.VeryEasy  => "Very Easy",
            ExperienceDifficulty.Easy      => "Easy",
            ExperienceDifficulty.Hard      => "Hard",
            ExperienceDifficulty.VeryHard  => "Very Hard",
            ExperienceDifficulty.Tough     => "Tough",      // 신규 추가
            ExperienceDifficulty.VeryTough => "Very Tough", // 신규 추가
            _ => "Unknown"
        };
    }

    // 난이도별 스프라이트 반환 (EX-04-1 대응)
    private Sprite GetDifficultySprite(ExperienceDifficulty difficulty)
    {
        return difficulty switch
        {
            ExperienceDifficulty.VeryEasy  => iconVeryEasy,
            ExperienceDifficulty.Easy      => iconEasy,
            ExperienceDifficulty.Hard      => iconHard,
            ExperienceDifficulty.VeryHard  => iconVeryHard,
            ExperienceDifficulty.Tough     => iconTough,     // 신규 추가
            ExperienceDifficulty.VeryTough => iconVeryTough, // 신규 추가
            _ => iconVeryEasy
        };
    }
}
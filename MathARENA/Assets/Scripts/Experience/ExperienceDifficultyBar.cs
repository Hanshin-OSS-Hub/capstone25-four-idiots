using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceDifficultyBar : MonoBehaviour
{
    [SerializeField] private TMP_Text difficultyText; // "Very Easy" 등
    [SerializeField] private Image difficultyIcon;    // 이모지/아이콘

    private void Start()
    {
        ApplyDifficulty(ExperienceSession.CurrentDifficulty);
    }

    public void ApplyDifficulty(ExperienceDifficulty difficulty)
    {
        // 텍스트
        if (difficultyText != null)
        {
            switch (difficulty)
            {
                case ExperienceDifficulty.VeryEasy:
                    difficultyText.text = "Very Easy";
                    break;
                case ExperienceDifficulty.Easy:
                    difficultyText.text = "Easy";
                    break;
                case ExperienceDifficulty.Hard:
                    difficultyText.text = "Hard";
                    break;
                case ExperienceDifficulty.VeryHard:
                    difficultyText.text = "Very Hard";
                    break;
            }
        }

        // 아이콘/색상 (원하면)
        if (difficultyIcon != null)
        {
            // 일단 난이도에 따라 색만 바꿔두자 (아이콘은 나중에)
            Color color = Color.white;

            switch (difficulty)
            {
                case ExperienceDifficulty.VeryEasy:
                    color = new Color(0.9f, 1.0f, 0.6f); // 연녹색
                    break;
                case ExperienceDifficulty.Easy:
                    color = new Color(0.7f, 1.0f, 0.7f);
                    break;
                case ExperienceDifficulty.Hard:
                    color = new Color(1.0f, 0.7f, 0.5f);
                    break;
                case ExperienceDifficulty.VeryHard:
                    color = new Color(1.0f, 0.5f, 0.5f);
                    break;
            }

            difficultyIcon.color = color;
        }
    }
}

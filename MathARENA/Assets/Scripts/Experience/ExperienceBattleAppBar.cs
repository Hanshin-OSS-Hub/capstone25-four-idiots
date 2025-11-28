using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//지금은 기능만 필요한 상태라
//시간 끝나면 isRunning=false로 멈추기만 하고
//실제 패배 처리/결과 화면은 TODO로 남겨둔 상태다.

public class ExperienceBattleAppBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private TMP_Text titleText; // 왼쪽: "훈련장-개념이해"

    [SerializeField]
    private TMP_Text timerText; // 가운데: "5:00"

    [Header("Timer Settings")]
    [SerializeField]
    private int startMinutes = 5; // 시작 시간(분 단위)

    private float remainingTime; // 초 단위로 카운트다운
    private bool isRunning = true;

    private void Start()
    {
        // 시작 시간(분)을 초 단위로 변환
        remainingTime = startMinutes * 60f;

        SetupTitleByCategory();
        UpdateTimerText(); // 처음에 5:00 표시
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            // TODO: 시간 초과 처리 (자동 패배, 결과 화면 등)
        }

        UpdateTimerText();
    }

    private void SetupTitleByCategory()
    {
        if (titleText == null)
            return;

        // 현재 선택된 체험장 종목 가져오기
        ExperienceCategory category = ExperienceSession.CurrentCategory;

        string categoryKorean = category switch
        {
            ExperienceCategory.Concept => "개념이해",
            ExperienceCategory.Calc => "연산",
            ExperienceCategory.Idea => "발상",
            ExperienceCategory.Design => "설계",
            ExperienceCategory.Practice => "실전",
            _ => "개념이해",
        };

        // 여기만 고치면 됨
        titleText.text = $"체험장-{categoryKorean}";
        // 체험장 화면이라면 "체험장-{categoryKorean}" 로 바꾸면 되고
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        // 5:00, 4:59 형식으로 출력
        timerText.text = $"{minutes}:{seconds:00}";
    }

    // 나가기 버튼에서 호출
    public void OnClickExit()
    {
        // 시간 멈춤
        isRunning = false;

        // 체험장 선택 화면으로 이동
        SceneManager.LoadScene("05_ExperienceSelect");
    }
}

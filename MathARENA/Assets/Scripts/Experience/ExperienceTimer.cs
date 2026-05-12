using TMPro;
using UnityEngine;

public class ExperienceTimer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text timerText;
    private float currentTimer = 60f;
    private bool isTimerRunning = false;
    private ExperienceBattleController controller;

    private void Awake()
    {
        controller = FindFirstObjectByType<ExperienceBattleController>();
    }

    private void Update()
    {
        if (!isTimerRunning)
            return;

        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(currentTimer).ToString();

            if (currentTimer <= 0)
            {
                currentTimer = 0;
                isTimerRunning = false;
                controller?.OnTimeOut(); // 여기가 실행되면 하트가 깎입니다.
            }
        }
    }

    public void ResetTimer(float time)
    {
        isTimerRunning = false;
        currentTimer = time;
        if (timerText != null)
            timerText.text = time.ToString();
        isTimerRunning = true;
        Debug.Log($"<color=cyan>[시계]</color> {time}초 리셋 및 시작");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("<color=red>[시계]</color> 정지됨");
    }
}

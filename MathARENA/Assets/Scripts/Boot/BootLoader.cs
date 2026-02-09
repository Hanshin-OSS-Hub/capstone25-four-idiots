using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public sealed class BootLoader : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "01_Login";

    [Header("UI (Optional)")]
    [SerializeField] private GameObject loadingRoot;     // Panel_LoadingRoot
    [SerializeField] private Slider progressSlider;      // 있으면 연결
    [SerializeField] private TMP_Text progressText;      // 있으면 연결 (ex: "Loading 45%")

    [Header("Timing")]
    [SerializeField] private float minShowTimeSec = 0.6f; // 너무 깜빡이지 않게 최소 표시시간
    [SerializeField] private float progressSmoothSpeed = 6f;

    private Coroutine _bootRoutine;
    private bool _isQuitting;

    private void Awake()
    {
        // 중복 실행 방지(실수로 BootLoader가 2개 생겨도 1개만 수행되게)
        if (_bootRoutine != null) return;

        if (loadingRoot != null)
            loadingRoot.SetActive(true);
    }

    private void OnEnable()
    {
        Application.quitting += OnAppQuitting;
    }

    private void OnDisable()
    {
        Application.quitting -= OnAppQuitting;

        // 코루틴 누수 방지
        if (_bootRoutine != null)
        {
            StopCoroutine(_bootRoutine);
            _bootRoutine = null;
        }
    }

    private void Start()
    {
        _bootRoutine = StartCoroutine(BootRoutine());
    }

    private void OnAppQuitting()
    {
        _isQuitting = true;
    }

    private IEnumerator BootRoutine()
    {
        // (1) 최소 노출 시간 보장
        float startTime = Time.unscaledTime;

        // (2) 초기화 작업(지금은 더미, 나중에 서버/데이터 로드 넣는 자리)
        // 예: PlayerPrefs 로드, 로컬 설정 초기화, Addressables 초기화 등
        yield return null;

        // (3) 다음 씬 Async 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        if (op == null)
        {
            Debug.LogError($"[BootLoader] LoadSceneAsync failed: {nextSceneName}");
            yield break;
        }

        // allowSceneActivation=false로 두고 0.9까지 로딩 후, 연출 끝나면 활성화
        op.allowSceneActivation = false;

        float shownProgress = 0f;

        while (!op.isDone)
        {
            if (_isQuitting) yield break;

            // Unity AsyncOperation.progress는 0~0.9까지만 올라가고, allowSceneActivation=true 후 마무리됨
            float target = Mathf.Clamp01(op.progress / 0.9f);

            shownProgress = Mathf.Lerp(shownProgress, target, Time.unscaledDeltaTime * progressSmoothSpeed);

            // UI 반영
            if (progressSlider != null)
                progressSlider.value = shownProgress;

            if (progressText != null)
            {
                int pct = Mathf.RoundToInt(shownProgress * 100f);
                progressText.text = $"Loading {pct}%";
            }

            // 로딩이 사실상 끝(0.9) + 최소 표시 시간 경과 + UI도 거의 100% 도달하면 씬 활성화
            bool loadReady = op.progress >= 0.9f;
            bool minTimePassed = (Time.unscaledTime - startTime) >= minShowTimeSec;
            bool uiCaughtUp = shownProgress >= 0.995f;

            if (loadReady && minTimePassed && uiCaughtUp)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

using MathArena.Network; // 에서 정의한 네임스페이스
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SignUpStepController : MonoBehaviour
{
    [Header("Step Panels")]
    [SerializeField]
    private GameObject panelTermsPrivacy;

    [SerializeField]
    private GameObject panelTermsService;

    [SerializeField]
    private GameObject panelSignUpForm;

    [Header("Privacy Terms UI")]
    [SerializeField]
    private Toggle togglePrivacyAgree;

    [SerializeField]
    private Button buttonPrivacyPrev;

    [SerializeField]
    private Button buttonPrivacyNext;

    [Header("Service Terms UI")]
    [SerializeField]
    private Toggle toggleServiceAgree;

    [SerializeField]
    private Button buttonServicePrev;

    [SerializeField]
    private Button buttonServiceNext;

    [Header("Form UI (Registration Inputs)")]
    [SerializeField]
    private TMP_InputField idInput;

    [SerializeField]
    private TMP_InputField pwInput;

    [SerializeField]
    private TMP_InputField pwConfirmInput;

    [SerializeField]
    private TMP_InputField nicknameInput;

    [SerializeField]
    private TMP_InputField emailInput;

    [SerializeField]
    private TMP_InputField phoneInput;

    [SerializeField]
    private Button buttonFormPrev;

    [SerializeField]
    private Button buttonFormSubmit; // [신규] 최종 가입 버튼

    [Header("Common References")]
    [SerializeField]
    private GameObject popupRoot;

    private enum Step
    {
        Privacy,
        Service,
        Form,
    }

    private Step _step;

    private void OnEnable()
    {
        ShowStep(Step.Privacy); // 팝업 켜질 때 항상 약관부터 시작

        // 리스너 등록
        togglePrivacyAgree.onValueChanged.AddListener(OnPrivacyAgreeChanged);
        toggleServiceAgree.onValueChanged.AddListener(OnServiceAgreeChanged);

        buttonPrivacyPrev.onClick.AddListener(OnPrivacyPrev);
        buttonPrivacyNext.onClick.AddListener(() => ShowStep(Step.Service));

        buttonServicePrev.onClick.AddListener(() => ShowStep(Step.Privacy));
        buttonServiceNext.onClick.AddListener(() => ShowStep(Step.Form));

        buttonFormPrev.onClick.AddListener(() => ShowStep(Step.Service));
        buttonFormSubmit.onClick.AddListener(OnClickCompleteRegister); // 서버 전송 버튼 연결
    }

    private void OnDisable()
    {
        // 중복 방지를 위해 리스너 제거
        togglePrivacyAgree.onValueChanged.RemoveAllListeners();
        toggleServiceAgree.onValueChanged.RemoveAllListeners();
        buttonPrivacyPrev.onClick.RemoveAllListeners();
        buttonPrivacyNext.onClick.RemoveAllListeners();
        buttonServicePrev.onClick.RemoveAllListeners();
        buttonServiceNext.onClick.RemoveAllListeners();
        buttonFormPrev.onClick.RemoveAllListeners();
        buttonFormSubmit.onClick.RemoveAllListeners();
    }

    private void ShowStep(Step step)
    {
        _step = step;
        panelTermsPrivacy.SetActive(step == Step.Privacy);
        panelTermsService.SetActive(step == Step.Service);
        panelSignUpForm.SetActive(step == Step.Form);

        if (step == Step.Privacy)
            OnPrivacyAgreeChanged(togglePrivacyAgree.isOn);
        if (step == Step.Service)
            OnServiceAgreeChanged(toggleServiceAgree.isOn);
    }

    private void OnPrivacyAgreeChanged(bool on) => buttonPrivacyNext.interactable = on;

    private void OnServiceAgreeChanged(bool on) => buttonServiceNext.interactable = on;

    private void OnPrivacyPrev()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    // ***** [핵심] 실제 서버에 회원가입 요청을 보내는 함수 *****
    public void OnClickCompleteRegister()
    {
        // 1. 유효성 검사 (비밀번호 확인 등)
        if (pwInput.text != pwConfirmInput.text)
        {
            Debug.LogError("비밀번호가 일치하지 않습니다.");
            return;
        }

        // 2. 서버 규격에 맞게 데이터 구성
        RegisterRequest regData = new RegisterRequest
        {
            id = idInput.text,
            pw = pwInput.text,
            pw_confirm = pwConfirmInput.text,
            nickname = nicknameInput.text,
            email = emailInput.text,
            phone = phoneInput.text,
            auth_id = "VERIFIED_ID_123", // 임시: 나중에 휴대전화 인증 성공 시 받은 ID를 넣으세요
        };

        // 3. NetworkManager를 통해 POST 요청 전송
        NetworkManager.Instance.PostRequest<AuthResponse<object>>(
            "/v1/auth/register",
            regData,
            (res) =>
            {
                if (res.success)
                {
                    Debug.Log("회원가입 성공! 이제 로그인해 보세요.");
                    if (popupRoot != null)
                        popupRoot.SetActive(false); // 가입 성공 시 팝업 닫기
                }
                else
                {
                    Debug.LogError($"가입 실패: {res.error.message}");
                }
            },
            (err) => Debug.LogError($"네트워크 오류: {err}")
        );
    }
}

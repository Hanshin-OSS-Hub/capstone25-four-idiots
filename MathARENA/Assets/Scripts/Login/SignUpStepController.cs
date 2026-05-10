using System.Collections.Generic;
using MathArena.Network;
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
    private Button buttonFormSubmit; // 최종 가입 버튼

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
        ShowStep(Step.Privacy);

        // 약관 동의 리스너
        togglePrivacyAgree.onValueChanged.AddListener(OnPrivacyAgreeChanged);
        toggleServiceAgree.onValueChanged.AddListener(OnServiceAgreeChanged);

        // 단계 이동 리스너
        buttonPrivacyPrev.onClick.AddListener(OnPrivacyPrev);
        buttonPrivacyNext.onClick.AddListener(() => ShowStep(Step.Service));
        buttonServicePrev.onClick.AddListener(() => ShowStep(Step.Privacy));
        buttonServiceNext.onClick.AddListener(() => ShowStep(Step.Form));
        buttonFormPrev.onClick.AddListener(() => ShowStep(Step.Service));

        // --- [추가] 모든 입력 필드가 바뀔 때마다 버튼 상태를 체크하도록 리스너 등록 ---
        idInput.onValueChanged.AddListener(_ => UpdateSubmitButtonState());
        pwInput.onValueChanged.AddListener(_ => UpdateSubmitButtonState());
        pwConfirmInput.onValueChanged.AddListener(_ => UpdateSubmitButtonState());
        nicknameInput.onValueChanged.AddListener(_ => UpdateSubmitButtonState());
        // 이메일과 전화번호는 선택사항이라면 아래 두 줄은 빼도 됩니다.
        emailInput.onValueChanged.AddListener(_ => UpdateSubmitButtonState());
        phoneInput.onValueChanged.AddListener(_ => UpdateSubmitButtonState());

        buttonFormSubmit.onClick.AddListener(OnClickCompleteRegister);

        // 초기 버튼 상태 설정
        UpdateSubmitButtonState();
    }

    private void OnDisable()
    {
        togglePrivacyAgree.onValueChanged.RemoveAllListeners();
        toggleServiceAgree.onValueChanged.RemoveAllListeners();
        buttonPrivacyPrev.onClick.RemoveAllListeners();
        buttonPrivacyNext.onClick.RemoveAllListeners();
        buttonServicePrev.onClick.RemoveAllListeners();
        buttonServiceNext.onClick.RemoveAllListeners();
        buttonFormPrev.onClick.RemoveAllListeners();
        buttonFormSubmit.onClick.RemoveAllListeners();

        // 입력 필드 리스너 제거
        idInput.onValueChanged.RemoveAllListeners();
        pwInput.onValueChanged.RemoveAllListeners();
        pwConfirmInput.onValueChanged.RemoveAllListeners();
        nicknameInput.onValueChanged.RemoveAllListeners();
        emailInput.onValueChanged.RemoveAllListeners();
        phoneInput.onValueChanged.RemoveAllListeners();
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
        if (step == Step.Form)
            UpdateSubmitButtonState(); // 양식 단계로 올 때도 체크
    }

    private void OnPrivacyAgreeChanged(bool on) => buttonPrivacyNext.interactable = on;

    private void OnServiceAgreeChanged(bool on) => buttonServiceNext.interactable = on;

    // --- [핵심 추가] 최종 가입 버튼의 활성화 여부를 실시간으로 판단하는 함수 ---
    private void UpdateSubmitButtonState()
    {
        // 1. 필수 입력 확인 (아이디, 비번, 비번확인, 닉네임)
        bool hasID = !string.IsNullOrEmpty(idInput.text);
        bool hasPW = !string.IsNullOrEmpty(pwInput.text);
        bool hasPWConfirm = !string.IsNullOrEmpty(pwConfirmInput.text);
        bool hasNickname = !string.IsNullOrEmpty(nicknameInput.text);

        // 2. 비밀번호 일치 확인
        bool isPasswordSame = (pwInput.text == pwConfirmInput.text);

        // 3. 버튼 활성화 (모든 필수 항목이 있고 비밀번호가 일치할 때)
        buttonFormSubmit.interactable =
            hasID && hasPW && hasPWConfirm && hasNickname && isPasswordSame;
    }

    private void OnPrivacyPrev()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    // SignUpStepController.cs 의 OnClickCompleteRegister 함수 수정

    public void OnClickCompleteRegister()
    {
        // 1. 비밀번호 확인
        if (pwInput.text != pwConfirmInput.text)
        {
            Debug.LogError("비밀번호가 일치하지 않습니다.");
            return;
        }

        // 2. 서버로 보낼 데이터 구성 (phone, auth_id 삭제)
        // RegisterRequest 클래스 정의 자체에서도 이 필드들을 지우거나 비워두어야 합니다.
        RegisterRequest regData = new RegisterRequest
        {
            id = idInput.text,
            pw = pwInput.text,
            pw_confirm = pwConfirmInput.text,
            nickname = nicknameInput.text,
            email = emailInput.text,
            // phone과 auth_id는 아예 작성하지 않습니다.
        };

        // 3. 서버 전송
        NetworkManager.Instance.PostRequest<AuthResponse<object>>(
            "/v1/auth/register",
            regData,
            (res) =>
            {
                if (res.success)
                {
                    Debug.Log("회원가입 성공!");
                    if (popupRoot != null)
                        popupRoot.SetActive(false);
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

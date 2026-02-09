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

    [Header("Form UI")]
    [SerializeField]
    private Button buttonFormPrev;

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
        // 초기 화면은 Privacy
        ShowStep(Step.Privacy);

        // 약관 토글 → Next 활성
        if (togglePrivacyAgree != null)
            togglePrivacyAgree.onValueChanged.AddListener(OnPrivacyAgreeChanged);
        if (toggleServiceAgree != null)
            toggleServiceAgree.onValueChanged.AddListener(OnServiceAgreeChanged);

        // 버튼 연결
        if (buttonPrivacyPrev != null)
            buttonPrivacyPrev.onClick.AddListener(OnPrivacyPrev);
        if (buttonPrivacyNext != null)
            buttonPrivacyNext.onClick.AddListener(() => ShowStep(Step.Service));

        if (buttonServicePrev != null)
            buttonServicePrev.onClick.AddListener(() => ShowStep(Step.Privacy));
        if (buttonServiceNext != null)
            buttonServiceNext.onClick.AddListener(() => ShowStep(Step.Form));

        if (buttonFormPrev != null)
            buttonFormPrev.onClick.AddListener(() => ShowStep(Step.Service));

        // 초기 Next 상태 반영
        OnPrivacyAgreeChanged(togglePrivacyAgree != null && togglePrivacyAgree.isOn);
        OnServiceAgreeChanged(toggleServiceAgree != null && toggleServiceAgree.isOn);
    }

    private void OnDisable()
    {
        // 중복 리스너/누수 방지
        if (togglePrivacyAgree != null)
            togglePrivacyAgree.onValueChanged.RemoveListener(OnPrivacyAgreeChanged);
        if (toggleServiceAgree != null)
            toggleServiceAgree.onValueChanged.RemoveListener(OnServiceAgreeChanged);

        if (buttonPrivacyPrev != null)
            buttonPrivacyPrev.onClick.RemoveListener(OnPrivacyPrev);
        if (buttonPrivacyNext != null)
            buttonPrivacyNext.onClick.RemoveAllListeners();

        if (buttonServicePrev != null)
            buttonServicePrev.onClick.RemoveAllListeners();
        if (buttonServiceNext != null)
            buttonServiceNext.onClick.RemoveAllListeners();

        if (buttonFormPrev != null)
            buttonFormPrev.onClick.RemoveAllListeners();
    }

    private void ShowStep(Step step)
    {
        _step = step;

        if (panelTermsPrivacy != null)
            panelTermsPrivacy.SetActive(step == Step.Privacy);
        if (panelTermsService != null)
            panelTermsService.SetActive(step == Step.Service);
        if (panelSignUpForm != null)
            panelSignUpForm.SetActive(step == Step.Form);

        // Step 진입 시 Next 상태 업데이트(토글이 이미 체크된 경우 대비)
        if (step == Step.Privacy)
            OnPrivacyAgreeChanged(togglePrivacyAgree != null && togglePrivacyAgree.isOn);

        if (step == Step.Service)
            OnServiceAgreeChanged(toggleServiceAgree != null && toggleServiceAgree.isOn);
    }

    private void OnPrivacyAgreeChanged(bool on)
    {
        if (buttonPrivacyNext != null)
            buttonPrivacyNext.interactable = on;
    }

    private void OnServiceAgreeChanged(bool on)
    {
        if (buttonServiceNext != null)
            buttonServiceNext.interactable = on;
    }

    private void OnPrivacyPrev()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }
}

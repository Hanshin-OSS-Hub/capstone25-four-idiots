using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelPopupRoot;

    [Header("Popups")]
    [SerializeField] private GameObject panelSignUpPopup;
    [SerializeField] private GameObject panelFindAccountPopup;

    /// <summary>
    /// 모든 팝업 끄기
    /// </summary>
    private void HideAllPopups()
    {
        if (panelSignUpPopup != null)
            panelSignUpPopup.SetActive(false);

        if (panelFindAccountPopup != null)
            panelFindAccountPopup.SetActive(false);
    }

    /// <summary>
    /// 회원가입 팝업 열기
    /// </summary>
    public void ShowSignUp()
    {
        if (panelPopupRoot != null)
            panelPopupRoot.SetActive(true);

        HideAllPopups();

        if (panelSignUpPopup != null)
            panelSignUpPopup.SetActive(true);
    }

    /// <summary>
    /// 계정찾기 팝업 열기
    /// </summary>
    public void ShowFindAccount()
    {
        if (panelPopupRoot != null)
            panelPopupRoot.SetActive(true);

        HideAllPopups();

        if (panelFindAccountPopup != null)
            panelFindAccountPopup.SetActive(true);
    }

    /// <summary>
    /// 전체 팝업 닫기 (X 버튼용)
    /// </summary>
    public void ClosePopupRoot()
    {
        if (panelPopupRoot != null)
            panelPopupRoot.SetActive(false);

        HideAllPopups();
    }
}
using TMPro;
using UnityEngine;

public class FindAccountTabController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelFindId;
    [SerializeField] private GameObject panelFindPw;

    [Header("Optional Description Text")]
    [SerializeField] private TMP_Text descText;

    [Header("Descriptions")]
    [TextArea]
    [SerializeField] private string findIdDescription = "아이디를 찾을 수 있습니다.";

    [TextArea]
    [SerializeField] private string findPwDescription = "비밀번호를 찾을 수 있습니다.";

    [Header("Optional Tab Visual")]
    [SerializeField] private GameObject findIdSelectedObject;
    [SerializeField] private GameObject findIdUnselectedObject;
    [SerializeField] private GameObject findPwSelectedObject;
    [SerializeField] private GameObject findPwUnselectedObject;

    private void Start()
    {
        ShowFindId();
    }

    public void ShowFindId()
    {
        if (panelFindId != null)
            panelFindId.SetActive(true);

        if (panelFindPw != null)
            panelFindPw.SetActive(false);

        if (descText != null)
            descText.text = findIdDescription;

        UpdateTabVisual(isFindIdSelected: true);
    }

    public void ShowFindPw()
    {
        if (panelFindId != null)
            panelFindId.SetActive(false);

        if (panelFindPw != null)
            panelFindPw.SetActive(true);

        if (descText != null)
            descText.text = findPwDescription;

        UpdateTabVisual(isFindIdSelected: false);
    }

    private void UpdateTabVisual(bool isFindIdSelected)
    {
        if (findIdSelectedObject != null)
            findIdSelectedObject.SetActive(isFindIdSelected);

        if (findIdUnselectedObject != null)
            findIdUnselectedObject.SetActive(!isFindIdSelected);

        if (findPwSelectedObject != null)
            findPwSelectedObject.SetActive(!isFindIdSelected);

        if (findPwUnselectedObject != null)
            findPwUnselectedObject.SetActive(isFindIdSelected);
    }
}
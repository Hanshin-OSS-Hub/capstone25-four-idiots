using System.Collections;
using UnityEngine;

public class KeyboardAreaHandler : MonoBehaviour
{
    [Header("조정할 UI 패널 (Panel_Root)")]
    [SerializeField]
    private RectTransform targetPanel;

    [Header("추가 여백 (UI 단위)")]
    [SerializeField]
    private float bottomMargin = 50f; // 이 값이 이제 사용됩니다!

    [Header("이동 속도")]
    [SerializeField]
    private float smoothSpeed = 10f;

    private Vector2 originalPosition;
    private float targetY;

    void Start()
    {
        if (targetPanel != null)
            originalPosition = targetPanel.anchoredPosition;
    }

    void Update()
    {
        if (TouchScreenKeyboard.visible)
        {
            float keyboardHeight = GetKeyboardHeight();

            // [수정] 키보드 높이에 우리가 설정한 여백(bottomMargin)을 더해줍니다.
            targetY = originalPosition.y + (keyboardHeight / GetCanvasScale()) + bottomMargin;
        }
        else
        {
            targetY = originalPosition.y;
        }

        Vector2 targetPos = new Vector2(originalPosition.x, targetY);
        targetPanel.anchoredPosition = Vector2.Lerp(
            targetPanel.anchoredPosition,
            targetPos,
            Time.deltaTime * smoothSpeed
        );
    }

    private float GetKeyboardHeight()
    {
#if UNITY_EDITOR
        return 0f;
#elif UNITY_ANDROID || UNITY_IOS
        return TouchScreenKeyboard.area.height;
#else
        return 0f;
#endif
    }

    private float GetCanvasScale()
    {
        Canvas canvas = targetPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
            return 1f;
        return canvas.rootCanvas.transform.localScale.y;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField]
    private RawImage displayImage;

    [SerializeField]
    private int textureSize = 128;

    [SerializeField]
    private int brushSize = 3;

    [SerializeField]
    private Color brushColor = Color.black; // 사용자가 보는 붓 색상

    private Texture2D drawingTexture;

    void Start()
    {
        drawingTexture = new Texture2D(textureSize, textureSize);
        drawingTexture.filterMode = FilterMode.Bilinear;
        ClearCanvas();

        if (displayImage != null)
        {
            displayImage.texture = drawingTexture;
            displayImage.color = Color.white; // 배경 UI가 투명하게 보이도록 기본값 설정
        }
    }

    public void ClearCanvas()
    {
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color(0, 0, 0, 0); // 투명 배경
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();
    }

    public void OnDrag(PointerEventData eventData) => Draw(eventData.position);

    public void OnPointerDown(PointerEventData eventData) => Draw(eventData.position);

    private void Draw(Vector2 screenPos)
    {
        if (displayImage == null)
            return;

        // [복구된 로직] 화면 좌표를 텍스처 상의 x, y 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            displayImage.rectTransform,
            screenPos,
            null,
            out Vector2 localPos
        );

        Rect rect = displayImage.rectTransform.rect;
        float x = (localPos.x - rect.x) / rect.width * textureSize;
        float y = (localPos.y - rect.y) / rect.height * textureSize;

        // 변수 x, y가 이제 존재하므로 에러가 사라집니다.
        if (x >= 0 && x < textureSize && y >= 0 && y < textureSize)
        {
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    int px = (int)x + i;
                    int py = (int)y + j;
                    if (px >= 0 && px < textureSize && py >= 0 && py < textureSize)
                    {
                        // 붓 색상을 적용 (검은색)
                        drawingTexture.SetPixel(px, py, brushColor);
                    }
                }
            }
            drawingTexture.Apply();
        }
    }

    public Texture2D GetCapturedTexture() => drawingTexture;
}

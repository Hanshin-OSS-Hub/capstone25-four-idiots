using Unity.InferenceEngine;

using UnityEngine;
using UnityEngine.UI;

public class SentisOCRManager : MonoBehaviour
{
    [SerializeField]
    private ModelAsset modelAsset;

    [SerializeField]
    private RawImage debugDisplay; // AI가 보는 이미지를 확인할 디버그 뷰 (선택)

    private Model runtimeModel;
    private Worker worker;

    void Start()
    {
        if (modelAsset != null)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, BackendType.GPUCompute);
        }
    }

    public int PredictDigit(Texture2D drawingTexture)
    {
        if (worker == null)
            return -1;

        Texture2D preprocessed = PreprocessTexture(drawingTexture);

        TensorShape shape = new TensorShape(1, 1, 28, 28);
        using Tensor<float> inputTensor = new Tensor<float>(shape);
        TextureConverter.ToTensor(preprocessed, inputTensor, new TextureTransform());

        worker.Schedule(inputTensor);
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        Destroy(preprocessed);
        return GetArgMax(outputTensor);
    }

    private Texture2D PreprocessTexture(Texture2D source)
    {
        Color[] srcPixels = source.GetPixels();
        int srcW = source.width;
        int srcH = source.height;

        int minX = srcW,
            minY = srcH,
            maxX = 0,
            maxY = 0;
        bool found = false;

        // 1. 글씨 영역 찾기 (Alpha가 0.1보다 크면 글씨로 간주)
        for (int y = 0; y < srcH; y++)
        {
            for (int x = 0; x < srcW; x++)
            {
                if (srcPixels[y * srcW + x].a > 0.1f)
                {
                    if (x < minX)
                        minX = x;
                    if (x > maxX)
                        maxX = x;
                    if (y < minY)
                        minY = y;
                    if (y > maxY)
                        maxY = y;
                    found = true;
                }
            }
        }

        if (!found)
            return new Texture2D(28, 28);

        // 2. 28x28 텍스처 생성
        Texture2D dest = new Texture2D(28, 28);
        Color[] destPixels = new Color[28 * 28];
        for (int i = 0; i < destPixels.Length; i++)
            destPixels[i] = Color.black;

        int rectW = maxX - minX + 1;
        int rectH = maxY - minY + 1;
        float scale = 20.0f / Mathf.Max(rectW, rectH);
        int scaledW = Mathf.RoundToInt(rectW * scale);
        int scaledH = Mathf.RoundToInt(rectH * scale);
        int startX = (28 - scaledW) / 2;
        int startY = (28 - scaledH) / 2;

        // 3. 리사이징 및 색상 반전 (UI 검은색 -> AI 흰색)
        for (int y = 0; y < scaledH; y++)
        {
            for (int x = 0; x < scaledW; x++)
            {
                float srcX = minX + (x / (float)scaledW) * rectW;
                float srcY = minY + (y / (float)scaledH) * rectH;
                Color c = source.GetPixelBilinear(srcX / srcW, srcY / srcH);

                // [중요] 알파값이 있으면 무조건 흰색으로 칠해서 AI에게 전달
                destPixels[(startY + y) * 28 + (startX + x)] =
                    (c.a > 0.2f) ? Color.white : Color.black;
            }
        }

        dest.SetPixels(destPixels);
        dest.Apply();

        if (debugDisplay != null)
            debugDisplay.texture = dest;
        return dest;
    }

    private int GetArgMax(Tensor<float> tensor)
    {
        if (tensor == null)
            return -1;
        float[] scores = tensor.DownloadToArray();
        int maxIndex = 0;
        float maxScore = scores[0];
        for (int i = 1; i < scores.Length; i++)
        {
            if (scores[i] > maxScore)
            {
                maxScore = scores[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}

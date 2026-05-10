using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class MathImageConverter
{
    // 배경 투명화 및 화질 개선 옵션이 포함된 API 주소입니다.
    private const string ApiUrl =
        "https://latex.codecogs.com/png.latex?\\dpi{150}\\bg_transparent ";

    public static IEnumerator LoadLatexImage(string latex, RawImage targetImage)
    {
        if (string.IsNullOrEmpty(latex))
            yield break;

        // URL에 포함될 수 없는 문자(+, \ 등)를 변환합니다.
        string encodedLatex = UnityWebRequest.EscapeURL(latex);
        string finalUrl = ApiUrl + encodedLatex;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(finalUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                targetImage.texture = texture;

                // 이미지 크기를 수식 비율에 맞게 자동으로 조절합니다.
                targetImage.SetNativeSize();
                targetImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"수식 로드 실패: {request.error}");
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class LatexTMPParser : MonoBehaviour
{
    [Header("텍스트를 띄울 UI")]
    public TextMeshProUGUI tmpText;

    /// <summary>
    /// 외부에서 문자열을 던져줄 때 호출하는 함수
    /// </summary>
    public void RenderTextWithLatex(string rawText)
    {
        StartCoroutine(ProcessTextCoroutine(rawText));
    }

    private IEnumerator ProcessTextCoroutine(string rawText)
    {
        MatchCollection matches = Regex.Matches(rawText, @"\$(.*?)\$");

        if (matches.Count == 0)
        {
            tmpText.text = rawText;
            yield break;
        }

        List<Texture2D> downloadedTextures = new List<Texture2D>();
        string finalText = rawText;

        for (int i = 0; i < matches.Count; i++)
        {
            string equation = matches[i].Groups[1].Value;

            // 서버 데이터 보정 (역슬래시 누락 시 추가)
            if (
                !equation.Contains("\\") && (equation.Contains("frac") || equation.Contains("sqrt"))
            )
                equation = "\\" + equation;

            // 임시 오브젝트를 만들어 MathImageConverter 활용
            GameObject tempObj = new GameObject("TempRaw");
            RawImage tempRaw = tempObj.AddComponent<RawImage>();

            // MathImageConverter의 정적 코루틴 호출
            yield return StartCoroutine(MathImageConverter.LoadLatexImage(equation, tempRaw));

            if (tempRaw.texture != null)
            {
                // Texture2D로 형변환하여 리스트에 추가
                downloadedTextures.Add((Texture2D)tempRaw.texture);
                finalText = finalText.Replace(matches[i].Value, $"<sprite index={i} yoffset=-12>");
            }
            Destroy(tempObj);
        }

        if (downloadedTextures.Count > 0)
        {
            tmpText.spriteAsset = CreateDynamicSpriteAsset(downloadedTextures);
        }

        tmpText.text = finalText;
    }

    /// <summary>
    /// TMP 버전 호환성을 맞춘 동적 SpriteAsset 생성 함수
    /// </summary>
    private TMP_SpriteAsset CreateDynamicSpriteAsset(List<Texture2D> textures)
    {
        TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();

        // 텍스처 할당
        Texture2D atlas = textures[0];
        spriteAsset.spriteSheet = atlas;

        // [수정 포인트] = new List... 대신 이미 존재하는 리스트에 하나씩 추가합니다.
        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D tex = textures[i];

            // 1. Glyph 설정 (이미지의 물리적 영역)
            TMP_SpriteGlyph glyph = new TMP_SpriteGlyph();
            glyph.index = (uint)i;
            glyph.metrics = new GlyphMetrics(tex.width, tex.height, 0, tex.height, tex.width);
            glyph.glyphRect = new GlyphRect(0, 0, tex.width, tex.height);
            glyph.scale = 1.0f;

            // 직접 할당(=) 대신 Add() 사용
            spriteAsset.spriteGlyphTable.Add(glyph);

            // 2. Character 설정 (텍스트에서 호출할 문자 정보)
            TMP_SpriteCharacter character = new TMP_SpriteCharacter((uint)i, glyph);
            character.name = "Equation_" + i;
            character.scale = 1.0f;

            // 직접 할당(=) 대신 Add() 사용
            spriteAsset.spriteCharacterTable.Add(character);
        }

        spriteAsset.UpdateLookupTables();
        return spriteAsset;
    }
}

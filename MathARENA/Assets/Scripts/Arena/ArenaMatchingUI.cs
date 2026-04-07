using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArenaMatchingUI : MonoBehaviour
{
    [Header("Opponent UI References")]
    [SerializeField]
    private TMP_Text opponentNicknameText; // 상대방 닉네임

    [SerializeField]
    private TMP_Text opponentBPDisplayText; // "설계 전투력 : 2,840 BP" 형태로 출력될 텍스트

    [SerializeField]
    private Image opponentIconImage; // 상대방 프로필 이미지

    [Header("Scene Navigation")]
    [SerializeField]
    private string battleSceneName = "11_ArenaBattle"; // 결투 씬 이름

    // 매칭 풀: 명세서에 따라 "나와 전투력 차이가 적은 순서"로 정렬된 리스트라고 가정합니다.
    private List<RankingEntryData> dummyOpponents = new List<RankingEntryData>();
    private int currentOpponentIndex = 0;

    void Start()
    {
        // 1. 상대방 리스트 초기화 (나중에 서버에서 내 BP와 비슷한 사람들을 받아오는 로직이 들어갈 곳)
        InitializeOpponents();

        // 2. 현재 선택된 종목 정보를 바탕으로 UI 첫 렌더링
        RefreshUI();
    }

    // [명세서 반영] 테스트를 위해 다양한 전투력을 가진 상대방 데이터를 생성합니다.
    private void InitializeOpponents()
    {
        // 실제로는 서버에서 쿼리해온 리스트가 들어옵니다.
        dummyOpponents.Add(new RankingEntryData { nickname = "엄준식", score = 2840 });
        dummyOpponents.Add(new RankingEntryData { nickname = "수학귀신", score = 3150 });
        dummyOpponents.Add(new RankingEntryData { nickname = "알고리즘맨", score = 1920 });
    }

    // UI 전체 새로고침
    public void RefreshUI()
    {
        if (dummyOpponents.Count == 0)
            return;

        // 1. 세션에서 현재 선택된 종목 정보를 가져옴
        ArenaCategory currentCat = ArenaSession.CurrentCategory;
        string catName = GetKoreanCategoryName(currentCat);

        // 2. 현재 인덱스의 상대방 데이터 호출
        var op = dummyOpponents[currentOpponentIndex];

        // 3. 상대 닉네임 설정
        if (opponentNicknameText != null)
            opponentNicknameText.text = op.nickname;

        // 4. [요구사항 반영] 종목 이름 + 상대 전투력 결합 (예: 설계 전투력 : 2,840 BP)
        if (opponentBPDisplayText != null)
        {
            opponentBPDisplayText.text = $"{catName} 전투력 : {op.score:N0} BP";
        }

        // 5. 상대 아이콘 이미지 설정
        if (opponentIconImage != null && op.profileIcon != null)
            opponentIconImage.sprite = op.profileIcon;
    }

    // 종목 코드를 한글 명칭으로 변환
    private string GetKoreanCategoryName(ArenaCategory cat)
    {
        return cat switch
        {
            ArenaCategory.Concept => "개념이해",
            ArenaCategory.Calc => "연산",
            ArenaCategory.Idea => "발상",
            ArenaCategory.Design => "설계",
            ArenaCategory.Practice => "실전",
            _ => "전투력",
        };
    }

    // --- 버튼 이벤트 함수 ---

    public void OnClickNextOpponent() // [다음 상대] 버튼
    {
        currentOpponentIndex = (currentOpponentIndex + 1) % dummyOpponents.Count;
        RefreshUI();
    }

    public void OnClickPrevOpponent() // [이전 상대] 버튼
    {
        currentOpponentIndex =
            (currentOpponentIndex - 1 + dummyOpponents.Count) % dummyOpponents.Count;
        RefreshUI();
    }

    public void OnClickStartBattle() // [배틀 시작] 버튼
    {
        // 선택된 상대의 정보를 세션에 기록 (배틀 씬에서 적의 실력으로 사용됨)
        var op = dummyOpponents[currentOpponentIndex];
        ArenaSession.OpponentId = op.nickname;
        ArenaSession.OpponentRating = op.score;

        SceneManager.LoadScene(battleSceneName);
    }
}

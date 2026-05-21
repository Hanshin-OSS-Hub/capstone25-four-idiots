using System.Collections.Generic;
using MathArena.Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArenaMatchingUI : MonoBehaviour
{
    [Header("Opponent UI References")]
    [SerializeField]
    private TMP_Text opponentNicknameText;

    [SerializeField]
    private TMP_Text opponentTierText; // 명세서 기반 티어 이름 (예: 매직 브론즈)

    [SerializeField]
    private TMP_Text opponentBPDisplayText; // [수정] AR이 아닌 'BP'로 표시

    [Header("Loading UI")]
    [SerializeField]
    private GameObject loadingOverlay;

    [Header("Scene Navigation")]
    [SerializeField]
    private string battleSceneName = "11_ArenaBattle";

    private List<RankingEntryData> opponentList = new List<RankingEntryData>();
    private List<ArenaMatchCandidate> serverCandidates = new List<ArenaMatchCandidate>();
    private int currentOpponentIndex = 0;

    void Start()
    {
        FetchRecommendations();
    }

    private void FetchRecommendations()
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        string currentCat = ArenaSession.CurrentCategory.ToString().ToLower();

        NetworkManager.Instance.FindMatch(
            currentCat,
            (res) =>
            {
                // 서버 데이터가 있으면 실제 데이터 사용, 없으면 더미
                if (res.success && res.data?.candidates != null && res.data.candidates.Count > 0)
                {
                    LoadRealOpponents(res.data.candidates);
                }
                else
                {
                    GenerateDummyOpponents();
                }

                if (loadingOverlay != null)
                    loadingOverlay.SetActive(false);
                RefreshUI();
            },
            (err) =>
            {
                GenerateDummyOpponents();
                if (loadingOverlay != null)
                    loadingOverlay.SetActive(false);
                RefreshUI();
            }
        );
    }

    // 실제 서버 candidates 데이터를 리스트에 매핑
    private void LoadRealOpponents(List<ArenaMatchCandidate> candidates)
    {
        serverCandidates = candidates; // ← 보관
        opponentList = new List<RankingEntryData>();
        foreach (var c in candidates)
        {
            opponentList.Add(
                new RankingEntryData
                {
                    nickname = c.opponent?.nickname ?? "User",
                    arena_rating = c.opponent?.arena_rating ?? 0,
                    score = c.opponent?.power ?? 0,
                }
            );
        }
        currentOpponentIndex = 0;
    }

    private void GenerateDummyOpponents()
    {
        opponentList = new List<RankingEntryData>();
        // 테스트 데이터: arena_rating은 승급용(내부), score는 전투력(BP)용
        opponentList.Add(
            new RankingEntryData
            {
                nickname = "User1 (Bot)",
                arena_rating = 254,
                score = 254,
            }
        );
        opponentList.Add(
            new RankingEntryData
            {
                nickname = "",
                arena_rating = 120,
                score = 120,
            }
        );
        opponentList.Add(
            new RankingEntryData
            {
                nickname = null,
                arena_rating = 450,
                score = 450,
            }
        );

        currentOpponentIndex = 0;
    }

    public void RefreshUI()
    {
        if (opponentList.Count == 0)
            return;

        var op = opponentList[currentOpponentIndex];
        string catName = GetKoreanCategoryName(ArenaSession.CurrentCategory);

        // 1. 닉네임: 없으면 "User"로 통일
        if (opponentNicknameText != null)
        {
            opponentNicknameText.text = string.IsNullOrEmpty(op.nickname) ? "User" : op.nickname;
        }

        // 2. 티어 텍스트: arena_rating 기반
        var info = TierManager.GetTierInfo(op.arena_rating);
        if (opponentTierText != null)
        {
            opponentTierText.text = info.fullName;
        }

        // 3. 전투력 표시: AR이 아닌 'BP' 단위 사용
        if (opponentBPDisplayText != null)
        {
            // BP는 100점 단위 리셋 없이 전체 수치를 보여줍니다.
            opponentBPDisplayText.text = $"{catName} 전투력 : {op.score} BP";
        }
    }

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

    public void OnClickNextOpponent()
    {
        currentOpponentIndex = (currentOpponentIndex + 1) % opponentList.Count;
        RefreshUI();
    }

    public void OnClickPrevOpponent()
    {
        currentOpponentIndex = (currentOpponentIndex - 1 + opponentList.Count) % opponentList.Count;
        RefreshUI();
    }

    public void OnClickStartBattle()
    {
        // 서버에서 받은 실제 candidate가 있으면 한 번에 세션에 저장
        if (serverCandidates != null && serverCandidates.Count > currentOpponentIndex)
        {
            ArenaSession.SetOpponent(serverCandidates[currentOpponentIndex]);
        }
        else
        {
            // 더미 데이터일 때는 기존 방식으로 최소 세팅만
            var selected = opponentList[currentOpponentIndex];
            ArenaSession.OpponentId = string.IsNullOrEmpty(selected.nickname)
                ? "User"
                : selected.nickname;
            ArenaSession.OpponentRating = selected.arena_rating;
            ArenaSession.OpponentPower = selected.score;
            ArenaSession.MatchId = "";
            ArenaSession.OpponentRecords =
                new System.Collections.Generic.List<MathArena.Network.OpponentRecord>();
            ArenaSession.LastQuestionOrder = 0;
        }

        SceneManager.LoadScene(battleSceneName);
    }
}

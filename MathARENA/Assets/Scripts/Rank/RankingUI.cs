using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingUI : MonoBehaviour
{
    [Header("My Rank UI")]
    [SerializeField]
    private TMP_Text myRankText;

    [SerializeField]
    private Image myProfileIconImage;

    [SerializeField]
    private TMP_Text myNicknameText;

    [SerializeField]
    private TMP_Text myLevelText;

    [SerializeField]
    private TMP_Text myScoreText;

    [SerializeField]
    private TMP_Text myArText;

    [Header("Ranking List")]
    [SerializeField]
    private Transform contentRoot;

    [SerializeField]
    private GameObject rankingRowPrefab;

    [Header("Leaderboard Settings")]
    [SerializeField]
    private int dummyPlayerCount = 20; // 표시할 더미 유저 수

    [SerializeField]
    private List<Sprite> otherProfileIcons = new List<Sprite>();

    [Header("Current Data")]
    [SerializeField]
    private List<RankingEntryData> debugEntries = new List<RankingEntryData>();

    private bool hasInitialized;

    public void OpenRanking()
    {
        // 서버 연결 없이 바로 더미데이터 생성
        if (!hasInitialized)
        {
            GenerateDebugRankingData();
            hasInitialized = true;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (contentRoot == null || rankingRowPrefab == null)
            return;

        ClearList();

        // 닉네임 순으로 정렬 (User1, User2...)
        List<RankingEntryData> sorted = debugEntries
            .OrderBy(x => x.nickname.Length) // 글자수 순 (User1, User10 구분용)
            .ThenBy(x => x.nickname)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].rank = i + 1;

            GameObject rowObj = Instantiate(rankingRowPrefab, contentRoot);
            RankingRowView rowView = rowObj.GetComponent<RankingRowView>();

            if (rowView != null)
            {
                rowView.Setup(sorted[i]); // 0AR이므로 모두 Normal Bronze로 나옵니다.
            }
        }
    }

    private void ClearList()
    {
        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }
    }

    // [핵심] 호건님이 요청하신 User1, 2... 생성 로직
    private void GenerateDebugRankingData()
    {
        debugEntries = new List<RankingEntryData>();

        for (int i = 0; i < dummyPlayerCount; i++)
        {
            debugEntries.Add(
                new RankingEntryData
                {
                    nickname = $"User{i + 1}", // 규칙적인 닉네임
                    ar = 0, // 모든 유저 0 AR 고정
                    level = 1,
                    score = 0,
                    profileIcon = GetRandomOtherProfileIcon(),
                    tier = RankingTierType.Bronze,
                }
            );
        }
    }

    private Sprite GetRandomOtherProfileIcon()
    {
        if (otherProfileIcons == null || otherProfileIcons.Count == 0)
            return null;
        return otherProfileIcons[Random.Range(0, otherProfileIcons.Count)];
    }
}

using System.Collections.Generic;
using System.Linq;
using MathArena.Network;
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

        List<RankingEntryData> sorted = debugEntries
            .OrderBy(x => x.nickname.Length)
            .ThenBy(x => x.nickname)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].rank = i + 1;
            GameObject rowObj = Instantiate(rankingRowPrefab, contentRoot);
            RankingRowView rowView = rowObj.GetComponent<RankingRowView>();
            if (rowView != null)
            {
                rowView.Setup(sorted[i]);
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

    
    private void GenerateDebugRankingData()
    {
        debugEntries = new List<RankingEntryData>();
        for (int i = 0; i < dummyPlayerCount; i++)
        {
            debugEntries.Add(new RankingEntryData
            {
                nickname = $"User{i + 1}",
                arena_rating = 0, // [수정] ar 대신 arena_rating 사용
                level = 1
            });
        }
    }

    private Sprite GetRandomOtherProfileIcon()
    {
        if (otherProfileIcons == null || otherProfileIcons.Count == 0)
            return null;
        return otherProfileIcons[Random.Range(0, otherProfileIcons.Count)];
    }
}

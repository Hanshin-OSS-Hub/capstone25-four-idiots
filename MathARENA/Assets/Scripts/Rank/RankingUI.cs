using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingUI : MonoBehaviour
{
    [Header("My Rank UI")]
    [SerializeField] private TMP_Text myRankText;
    [SerializeField] private Image myProfileIconImage;
    [SerializeField] private TMP_Text myNicknameText;
    [SerializeField] private TMP_Text myLevelText;
    [SerializeField] private TMP_Text myScoreText;
    [SerializeField] private TMP_Text myArText;

    [Header("Ranking List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject rankingRowPrefab;

    [Header("My Dummy Data")]
    [SerializeField] private string myNickname = "엄준식";
    [SerializeField] private int myLevel = 12;
    [SerializeField] private int myScore = 2840;
    [SerializeField] private int myAr = 70;
    [SerializeField] private Sprite myProfileIcon;

    [Header("Other Dummy Data")]
    [SerializeField] private int dummyPlayerCount = 99;
    [SerializeField] private List<Sprite> otherProfileIcons = new List<Sprite>();

    [Header("Generated Debug Data (Read Only 느낌으로 사용)")]
    [SerializeField] private List<RankingEntryData> debugEntries = new List<RankingEntryData>();

    private bool hasInitialized;

    public void OpenRanking()
    {
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
        {
            Debug.LogWarning("[RankingUI] contentRoot 또는 rankingRowPrefab 참조가 비어 있습니다.");
            return;
        }

        ClearList();

        List<RankingEntryData> sorted = debugEntries
            .OrderByDescending(x => x.ar)      // 아레나 포인트 우선
            .ThenByDescending(x => x.score)    // 동점이면 BP
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].rank = i + 1;
            sorted[i].tier = GetTierByArenaPoint(sorted[i].ar);
        }

        RankingEntryData myData = null;

        for (int i = 0; i < sorted.Count; i++)
        {
            GameObject rowObj = Instantiate(rankingRowPrefab, contentRoot);
            RankingRowView rowView = rowObj.GetComponent<RankingRowView>();

            if (rowView != null)
            {
                rowView.Setup(sorted[i]);
            }

            if (sorted[i].nickname == myNickname)
            {
                myData = sorted[i];
            }
        }

        RefreshMyRankCard(myData);
    }

    private RankingTierType GetTierByArenaPoint(int arenaPoint)
    {
        if (arenaPoint >= 90)
            return RankingTierType.Gold;

        if (arenaPoint >= 70)
            return RankingTierType.Silver;

        return RankingTierType.Bronze;
    }

    private void RefreshMyRankCard(RankingEntryData myData)
    {
        if (myData == null)
        {
            if (myRankText != null) myRankText.text = "-";
            if (myProfileIconImage != null) myProfileIconImage.sprite = null;
            if (myNicknameText != null) myNicknameText.text = "내 정보 없음";
            if (myLevelText != null) myLevelText.text = "-";
            if (myScoreText != null) myScoreText.text = "-";
            if (myArText != null) myArText.text = "-";
            return;
        }

        if (myRankText != null) myRankText.text = myData.rank.ToString();
        if (myProfileIconImage != null) myProfileIconImage.sprite = myData.profileIcon;
        if (myNicknameText != null) myNicknameText.text = myData.nickname;
        if (myLevelText != null) myLevelText.text = $"Lv.{myData.level}";
        if (myScoreText != null) myScoreText.text = $"{myData.score} BP";
        if (myArText != null) myArText.text = $"{myData.ar} AR";
    }

    private void ClearList()
    {
        if (contentRoot == null) return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    [ContextMenu("Generate Debug Ranking Data")]
    private void GenerateDebugRankingData()
    {
        debugEntries = new List<RankingEntryData>();

        // 1. 본인 데이터 먼저 추가
        RankingEntryData myEntry = new RankingEntryData
        {
            nickname = myNickname,
            level = myLevel,
            score = myScore,
            ar = myAr,
            profileIcon = myProfileIcon,
            tier = RankingTierType.Bronze
        };

        debugEntries.Add(myEntry);

        // 2. 다른 사람 데이터 추가
        for (int i = 0; i < dummyPlayerCount; i++)
        {
            RankingEntryData otherEntry = new RankingEntryData
            {
                nickname = GetDummyNickname(i),
                level = Random.Range(1, 31),
                score = Random.Range(1000, 10000),
                ar = Random.Range(30, 101),
                profileIcon = GetRandomOtherProfileIcon(),
                tier = RankingTierType.Bronze
            };

            debugEntries.Add(otherEntry);
        }
    }

    private string GetDummyNickname(int index)
    {
        string[] names =
        {
            "수학왕", "천재소년", "공부의신", "수포자탈출", "연산기저",
            "수학마스터", "공식가왕", "문제테일사", "정답률100", "암산명",
            "알고리즘킹", "미적분헌터", "확률도사", "도형장인", "함수전설",
            "공대전사", "해답사냥꾼", "개념폭격기", "시험만점", "정석파괴자"
        };

        string baseName = names[index % names.Length];
        int suffix = index / names.Length + 1;
        return $"{baseName}{suffix}";
    }

    private Sprite GetRandomOtherProfileIcon()
    {
        if (otherProfileIcons == null || otherProfileIcons.Count == 0)
            return null;

        int randomIndex = Random.Range(0, otherProfileIcons.Count);
        return otherProfileIcons[randomIndex];
    }
}
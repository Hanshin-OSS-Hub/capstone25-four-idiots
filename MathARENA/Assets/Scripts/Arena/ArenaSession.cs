using System.Collections.Generic;
using MathArena.Network;
using UnityEngine;

public static class ArenaSession
{
    public static ArenaCategory CurrentCategory = ArenaCategory.Concept;

    // 명세서 반영: 프로필에 저장된 종목별 실제 전투력
    public static int ConceptBP = 240;
    public static int CalcBP = 510;
    public static int IdeaBP = 120;
    public static int DesignBP = 430;
    public static int PracticeBP = 55;

    public static int GetPlayerBP(ArenaCategory category)
    {
        return category switch
        {
            ArenaCategory.Concept => ConceptBP,
            ArenaCategory.Calc => CalcBP,
            ArenaCategory.Idea => IdeaBP,
            ArenaCategory.Design => DesignBP,
            ArenaCategory.Practice => PracticeBP,
            _ => 0,
        };
    }

    // ── 상대방 기본 정보 ──────────────────────────────────────────────────
    public static string OpponentId;
    public static int OpponentRating;

    /// <summary>매칭 화면에서 선택한 상대의 전투력(BP)</summary>
    public static int OpponentPower;

    // ── 매치 식별자 ────────────────────────────────────────────────────────
    /// <summary>서버가 발급한 match_id (전투 씬에서 StartMatch / FinishMatch 에 사용)</summary>
    public static string MatchId;

    // ── 상대방 풀이 기록 (전투 판정의 핵심) ─────────────────────────────────
    /// <summary>
    /// 매칭 화면에서 FindMatch 응답으로 받은 상대의 문제별 기록.
    /// 전투 씬에서 각 문제마다 "내 풀이시간 vs 상대 풀이시간" 비교에 사용한다.
    /// </summary>
    public static List<OpponentRecord> OpponentRecords = new List<OpponentRecord>();

    /// <summary>
    /// 같은 상대와 재대결 시 이전에 마지막으로 출제된 문제 순서 번호.
    /// 전투 씬은 이 값 + 1 번째 문제부터 시작한다.
    /// </summary>
    public static int LastQuestionOrder = 0;

    // ── 초기화 헬퍼 ────────────────────────────────────────────────────────
    /// <summary>매칭 화면에서 상대를 선택할 때 호출. 관련 필드를 일괄 세팅한다.</summary>
    public static void SetOpponent(ArenaMatchCandidate candidate)
    {
        if (candidate == null)
            return;

        MatchId = candidate.match_id ?? "";
        OpponentId = candidate.opponent?.nickname ?? "User";
        OpponentRating = candidate.opponent?.arena_rating ?? 0;
        OpponentPower = candidate.opponent?.power ?? 0;
        OpponentRecords = candidate.opponent_records ?? new List<OpponentRecord>();
        LastQuestionOrder = candidate.last_question_order;
    }
}

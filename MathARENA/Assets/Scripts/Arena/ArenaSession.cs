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

    // [에러 해결] 함수 이름을 GetPlayerBP로 변경하고 매개변수 category를 받도록 수정합니다.
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

    // 상대방 정보
    public static string OpponentId;
    public static int OpponentRating;
}

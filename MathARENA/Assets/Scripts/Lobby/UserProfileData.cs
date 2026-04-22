using System;

[Serializable]
public class UserProfileData
{
    // [필수] SQL PROFILE 테이블 일치 필드
    public string nickname;
    public int arena_rating; // 티어 결정용 (기존 totalAR 대체)

    // 전투력 5종 (평균 전투력 계산용)
    public int cp_concept; // 개념이해
    public int cp_calc; // 연산
    public int cp_idea; // 발상
    public int cp_design; // 설계
    public int cp_practical; // 실전

    // [부가] UI 껍데기용
    public int gold;
    public int arenaTickets;
    public string illustrationId;
}

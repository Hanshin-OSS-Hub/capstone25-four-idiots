using UnityEngine;
using UnityEngine.SceneManagement;
using MathArena.Network;

public class LogoutController : MonoBehaviour
{
    /// <summary>
    /// 로그아웃 버튼의 OnClick 이벤트에 연결하세요.
    /// </summary>
    public void OnClickLogout()
    {
        Debug.Log("[Logout] 로그아웃을 시작합니다.");

        // 1. 서버 통신 토큰 초기화
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SetToken(null);
        }

        // 2. 모든 세션 데이터 초기화 (메모리 정리)
        ClearAllSessions();

        // 3. 로그인 씬으로 이동
        // 로그인 씬 이름이 "01_Login"인지 확인해 주세요.
        SceneManager.LoadScene("01_Login");
    }

    private void ClearAllSessions()
    {
        // 체험장 세션 초기화
        ExperienceSession.UserProfile = null;
        ExperienceSession.TotalExpScore = 0;
        ExperienceSession.CurrentQuestionCount = 0;
        
        // 아레나 세션 초기화
        ArenaSession.OpponentId = null;
        ArenaSession.OpponentRating = 0;
        
        // 훈련장 세션 초기화
        TrainingSession.CurrentCategory = TrainingCategory.Concept;
        
        Debug.Log("[Logout] 모든 세션 데이터가 초기화되었습니다.");
    }
}
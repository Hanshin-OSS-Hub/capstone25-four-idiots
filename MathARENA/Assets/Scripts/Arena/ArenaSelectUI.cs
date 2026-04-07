using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaSelectUI : MonoBehaviour
{
    [SerializeField]
    private string arenaMatchingSceneName = "10_Arena"; // 이동할 매칭 씬 이름

    // 각 종목 버튼의 On Click() 이벤트에 연결하세요.
    public void OnClickConcept() => SetCategoryAndLoad(ArenaCategory.Concept);

    public void OnClickCalc() => SetCategoryAndLoad(ArenaCategory.Calc);

    public void OnClickIdea() => SetCategoryAndLoad(ArenaCategory.Idea);

    public void OnClickDesign() => SetCategoryAndLoad(ArenaCategory.Design);

    public void OnClickPractice() => SetCategoryAndLoad(ArenaCategory.Practice);

    private void SetCategoryAndLoad(ArenaCategory category)
    {
        // 1. 선택한 카테고리를 정적 세션에 저장 (씬이 바뀌어도 유지됨)
        ArenaSession.CurrentCategory = category;

        // 2. 매칭 화면 씬으로 이동
        SceneManager.LoadScene(arenaMatchingSceneName);
    }
}

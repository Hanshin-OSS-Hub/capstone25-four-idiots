using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaSelectUI : MonoBehaviour
{
    [SerializeField]
    private string arenaBattleSceneName = "06_ExperienceBattle";

    // 아레나가 사용할 배틀 씬 이름
    // (공용 배틀 컨트롤러에서 mode = Arena로 설정돼 있어야 함)

    public void OnClickConcept()
    {
        ArenaSession.CurrentCategory = ArenaCategory.Concept;
        SceneManager.LoadScene(arenaBattleSceneName);
    }

    public void OnClickCalc()
    {
        ArenaSession.CurrentCategory = ArenaCategory.Calc;
        SceneManager.LoadScene(arenaBattleSceneName);
    }

    public void OnClickIdea()
    {
        ArenaSession.CurrentCategory = ArenaCategory.Idea;
        SceneManager.LoadScene(arenaBattleSceneName);
    }

    public void OnClickDesign()
    {
        ArenaSession.CurrentCategory = ArenaCategory.Design;
        SceneManager.LoadScene(arenaBattleSceneName);
    }

    public void OnClickPractice()
    {
        ArenaSession.CurrentCategory = ArenaCategory.Practice;
        SceneManager.LoadScene(arenaBattleSceneName);
    }
}

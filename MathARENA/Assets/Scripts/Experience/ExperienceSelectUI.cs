using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperienceSelectUI : MonoBehaviour
{
    // 공통 전투 씬 이름 (하나만 만든다)
    private const string BATTLE_SCENE_NAME = "06_ExperienceBattle";

    public void OnClickConcept()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Concept;
        SceneManager.LoadScene(BATTLE_SCENE_NAME);
    }

    public void OnClickCalc()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Calc;
        SceneManager.LoadScene(BATTLE_SCENE_NAME);
    }

    public void OnClickIdea()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Idea;
        SceneManager.LoadScene(BATTLE_SCENE_NAME);
    }

    public void OnClickDesign()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Design;
        SceneManager.LoadScene(BATTLE_SCENE_NAME);
    }

    public void OnClickPractice()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Practice;
        SceneManager.LoadScene(BATTLE_SCENE_NAME);
    }
}

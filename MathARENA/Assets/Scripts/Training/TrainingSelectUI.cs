using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingSelectUI : MonoBehaviour
{
    [SerializeField]
    private string trainingBattleSceneName = "08_TrainingBattle";

    public void OnClickConcept()
    {
        // TrainingSession 대신 ExperienceSession을 사용합니다.
        ExperienceSession.CurrentCategory = ExperienceCategory.Concept;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickCalc()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Calc;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickIdea()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Idea;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickDesign()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Design;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickPractice()
    {
        ExperienceSession.CurrentCategory = ExperienceCategory.Practice;
        SceneManager.LoadScene(trainingBattleSceneName);
    }
}

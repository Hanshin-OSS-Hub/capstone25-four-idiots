using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingSelectUI : MonoBehaviour
{
    [SerializeField]
    private string trainingBattleSceneName = "08_TrainingBattle"; // 네 실제 씬 이름으로 변경

    public void OnClickConcept()
    {
        TrainingSession.CurrentCategory = TrainingCategory.Concept;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickCalc()
    {
        TrainingSession.CurrentCategory = TrainingCategory.Calc;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickIdea()
    {
        TrainingSession.CurrentCategory = TrainingCategory.Idea;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickDesign()
    {
        TrainingSession.CurrentCategory = TrainingCategory.Design;
        SceneManager.LoadScene(trainingBattleSceneName);
    }

    public void OnClickPractice()
    {
        TrainingSession.CurrentCategory = TrainingCategory.Practice;
        SceneManager.LoadScene(trainingBattleSceneName);
    }
}

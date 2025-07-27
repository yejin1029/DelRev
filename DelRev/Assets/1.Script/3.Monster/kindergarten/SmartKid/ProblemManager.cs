using UnityEngine;

public class ProblemManager : MonoBehaviour
{
    public static ProblemManager Instance;
    private GameObject player;
    private int answer;

    void Awake()
    {
        Instance = this;
    }

    public void StartProblem(GameObject playerObj)
    {
        player = playerObj;
        int a = Random.Range(10, 100);
        int b = Random.Range(10, 100);
        answer = a + b;
        MathProblemUI.Instance.ShowProblem($"{a} + {b} = ?", answer, OnAnswerSubmitted);
    }

    void OnAnswerSubmitted(bool isCorrect)
    {
        if (isCorrect)
        {
            SmartKidAI smartKid = FindObjectOfType<SmartKidAI>();
            smartKid.ReleasePlayer();
        }
        else
        {
            // 문제 계속 시도
        }
    }
}

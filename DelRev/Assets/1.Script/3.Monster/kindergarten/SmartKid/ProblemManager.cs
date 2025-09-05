using UnityEngine;

public class ProblemManager : MonoBehaviour
{
    public static ProblemManager Instance;

    private MathProblemUI ui;
    private SmartKidAI currentKid;
    private GameObject currentPlayer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ui = FindObjectOfType<MathProblemUI>(true);
        if (ui != null)
        {
            ui.gameObject.SetActive(false);
            Debug.Log("[ProblemManager] MathProblemUI 찾음 → 시작 시 숨김");
        }
    }

    public void StartProblem(GameObject player, SmartKidAI kid)
    {
        currentPlayer = player;
        currentKid = kid;

        if (ui != null)
        {
            ui.gameObject.SetActive(true);
            ui.ShowNewProblem(OnSolved);
            Debug.Log("[ProblemManager] 문제 출제 시작!");
        }
    }

    private void OnSolved()
    {
        if (ui != null)
        {
            ui.gameObject.SetActive(false);
            Debug.Log("[ProblemManager] 문제 해결 → UI 닫음");
        }

        if (currentKid != null)
        {
            currentKid.ReleasePlayer();
            currentKid = null;
            Debug.Log("[ProblemManager] SmartKid 해방");
        }

        currentPlayer = null;
    }
}

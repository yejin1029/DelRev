using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MathProblemUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TMP_Text questionText;
    public TMP_InputField answerField;

    private int correctAnswer;
    private System.Action onSolvedCallback;

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        // 🔹 입력창 클릭하면 무조건 다시 포커스 강제
        if (answerField != null)
        {
            answerField.onSelect.AddListener((s) =>
            {
                ForceFocus();
            });
        }
    }

    public void ShowNewProblem(System.Action onSolved)
    {
        onSolvedCallback = onSolved;

        int a = Random.Range(10, 100);
        int b = Random.Range(10, 100);
        correctAnswer = a + b;

        if (questionText != null)
            questionText.text = $"{a} + {b} = ?";

        if (answerField != null)
        {
            answerField.text = "";
            ForceFocus(); // 🔹 문제 시작 시 강제 포커스
        }

        if (panel != null)
            panel.SetActive(true);

        Debug.Log($"[MathProblemUI] 새로운 문제 출제: {a} + {b} (정답: {correctAnswer})");
    }

    void Update()
    {
        if (panel != null && panel.activeSelf)
        {
            // 엔터로 제출
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnSubmitAnswer();
            }

            // ESC 눌러도 입력창 다시 선택 가능하게
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ForceFocus();
            }
        }
    }

    private void ForceFocus()
    {
        if (answerField != null)
        {
            EventSystem.current.SetSelectedGameObject(answerField.gameObject);
            answerField.OnPointerClick(new PointerEventData(EventSystem.current));
            answerField.ActivateInputField();
            Debug.Log("[MathProblemUI] 입력창 강제 포커스!");
        }
    }

    private void OnSubmitAnswer()
    {
        if (string.IsNullOrEmpty(answerField.text)) return;

        if (int.TryParse(answerField.text, out int playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                Debug.Log("[MathProblemUI] 정답!");
                CloseUI();
                onSolvedCallback?.Invoke();
            }
            else
            {
                Debug.Log("[MathProblemUI] 오답! 다시 시도하세요.");
                answerField.text = "";
                ForceFocus(); // 🔹 오답 시 다시 포커스
            }
        }
    }

    private void CloseUI()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}

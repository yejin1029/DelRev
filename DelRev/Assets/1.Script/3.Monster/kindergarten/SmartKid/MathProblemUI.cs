using UnityEngine;
using UnityEngine.UI;

public class MathProblemUI : MonoBehaviour
{
    public static MathProblemUI Instance;

    public GameObject panel;
    public Text questionText;
    public InputField answerField;

    private int correctAnswer;
    private System.Action<bool> onSubmit;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowProblem(string question, int answer, System.Action<bool> callback)
    {
        panel.SetActive(true);
        questionText.text = question;
        answerField.text = "";
        correctAnswer = answer;
        onSubmit = callback;
        answerField.ActivateInputField();
    }

    public void OnSubmitAnswer()
    {
        int userAnswer;
        bool parsed = int.TryParse(answerField.text, out userAnswer);
        bool isCorrect = parsed && userAnswer == correctAnswer;
        panel.SetActive(false);
        onSubmit?.Invoke(isCorrect);
    }
}

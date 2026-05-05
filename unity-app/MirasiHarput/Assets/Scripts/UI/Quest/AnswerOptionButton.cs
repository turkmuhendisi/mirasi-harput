using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerOptionButton : MonoBehaviour
{
    [SerializeField] TMP_Text answerText = null;
    [SerializeField] Button button = null;

    bool isCorrectAnswer;
    string currentAnswerText = string.Empty;
    Action<bool, string> onSelected;

    void Awake()
    {
        ResolveReferences();
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClicked);
    }

    void ResolveReferences()
    {
        if (answerText == null)
            answerText = GetComponentInChildren<TMP_Text>(true);

        if (button == null)
            button = GetComponent<Button>();
    }

    public void Setup(string answerTextValue, bool isCorrect, Action<bool, string> onSelectedCallback)
    {
        ResolveReferences();

        currentAnswerText = string.IsNullOrEmpty(answerTextValue) ? "-" : answerTextValue;
        isCorrectAnswer = isCorrect;
        onSelected = onSelectedCallback;

        if (answerText != null)
            answerText.text = currentAnswerText;

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
            button.onClick.AddListener(HandleClicked);
            button.interactable = true;
        }
    }

    public void SetInteractable(bool interactable)
    {
        ResolveReferences();
        if (button != null)
            button.interactable = interactable;
    }

    void HandleClicked()
    {
        onSelected?.Invoke(isCorrectAnswer, currentAnswerText);
    }
}

using System.Collections;
using TMPro;
using UnityEngine;

public class LocationToast : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup = null;
    [SerializeField] TMP_Text messageText = null;
    [SerializeField] float displaySeconds = 3f;

    Coroutine hideRoutine;

    public void Configure(CanvasGroup group, TMP_Text text)
    {
        canvasGroup = group;
        messageText = text;
    }

    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displaySeconds);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
        hideRoutine = null;
    }
}

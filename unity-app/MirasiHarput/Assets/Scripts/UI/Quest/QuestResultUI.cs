using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestResultUI : MonoBehaviour
{
    [SerializeField] CanvasGroup resultCanvasGroup = null;
    [SerializeField] TMP_Text resultTitleText = null;
    [SerializeField] TMP_Text rewardPointText = null;
    [SerializeField] TMP_Text badgeUnlockedText = null;
    [SerializeField] TMP_Text nextTargetText = null;
    [SerializeField] Button continueRouteButton = null;

    public event Action OnContinueRoute;

    public bool IsVisible { get; private set; }

    void Awake()
    {
        ResolveReferences();
        HideResult();
    }

    void OnEnable()
    {
        SubscribeButton();
    }

    void OnDisable()
    {
        UnsubscribeButton();
    }

    void ResolveReferences()
    {
        if (resultCanvasGroup == null)
            resultCanvasGroup = GetComponent<CanvasGroup>();

        if (continueRouteButton == null)
            continueRouteButton = GetComponentInChildren<Button>(true);
    }

    void SubscribeButton()
    {
        if (continueRouteButton == null)
            return;

        continueRouteButton.onClick.RemoveListener(HandleContinueClicked);
        continueRouteButton.onClick.AddListener(HandleContinueClicked);
    }

    void UnsubscribeButton()
    {
        if (continueRouteButton != null)
            continueRouteButton.onClick.RemoveListener(HandleContinueClicked);
    }

    public void ShowResult(QuestData quest, int rewardPoint, string badgeId, LocationData nextLocation)
    {
        ResolveReferences();

        var questTitle = quest != null ? quest.title : "Görev";
        SetText(resultTitleText, questTitle + " tamamlandı");
        SetText(rewardPointText, "Puan: +" + Mathf.Max(0, rewardPoint));
        SetText(badgeUnlockedText, "Rozet: " + (string.IsNullOrEmpty(badgeId) ? "-" : badgeId));
        SetText(nextTargetText, "Sonraki hedef: " + (nextLocation != null ? nextLocation.name : "Rota tamamlandı"));

        SetVisible(true);
    }

    public void HideResult()
    {
        SetVisible(false);
    }

    void HandleContinueClicked()
    {
        HideResult();
        OnContinueRoute?.Invoke();
    }

    void SetVisible(bool visible)
    {
        IsVisible = visible;

        if (resultCanvasGroup == null)
            return;

        resultCanvasGroup.alpha = visible ? 1f : 0f;
        resultCanvasGroup.interactable = visible;
        resultCanvasGroup.blocksRaycasts = visible;
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}

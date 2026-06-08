using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rota listesinde dinamik oluşturulan satır. Kendi buton prefab'ınızı tasarlayın.
/// </summary>
public class QrRouteListItem : MonoBehaviour
{
    [SerializeField] Button button = null;
    [SerializeField] TMP_Text labelText = null;

    void Awake()
    {
        ResolveReferences();
    }

    void ResolveReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (labelText == null)
            labelText = GetComponentInChildren<TMP_Text>(true);
    }

    public void Setup(string label, Action onSelected)
    {
        ResolveReferences();

        if (labelText != null)
            labelText.text = label;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onSelected != null)
                button.onClick.AddListener(() => onSelected());
        }
    }
}

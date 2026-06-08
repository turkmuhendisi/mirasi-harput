using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QrLocationTriggerDebugUI : MonoBehaviour
{
    [SerializeField] JsonDataLoader dataLoader = null;
    [SerializeField] QrLocationTriggerBridge triggerBridge = null;
    [SerializeField] QrCodeScanService scanService = null;
    [SerializeField] TMP_Text statusText = null;
    [SerializeField] TMP_Dropdown locationDropdown = null;
    [SerializeField] Button simulateScanButton = null;

    void OnEnable()
    {
        ResolveReferences();
        PopulateDropdown();
        BindButtons();
        RefreshStatus();
    }

    void OnDisable()
    {
        if (simulateScanButton != null)
            simulateScanButton.onClick.RemoveListener(HandleSimulateScanClicked);
    }

    void ResolveReferences()
    {
        if (dataLoader == null)
            dataLoader = JsonDataLoader.Instance != null ? JsonDataLoader.Instance : FindAnyObjectByType<JsonDataLoader>();

        if (triggerBridge == null)
            triggerBridge = FindAnyObjectByType<QrLocationTriggerBridge>();

        if (scanService == null)
            scanService = FindAnyObjectByType<QrCodeScanService>();
    }

    void PopulateDropdown()
    {
        if (locationDropdown == null || dataLoader == null || !dataLoader.HasQrRegistry || dataLoader.QrRegistry == null)
            return;

        locationDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        var entries = dataLoader.QrRegistry.locations;
        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry == null)
                continue;

            options.Add(entry.displayName + " (" + entry.locationId + ")");
        }

        locationDropdown.AddOptions(options);
    }

    void BindButtons()
    {
        if (simulateScanButton == null)
            return;

        simulateScanButton.onClick.RemoveListener(HandleSimulateScanClicked);
        simulateScanButton.onClick.AddListener(HandleSimulateScanClicked);
    }

    void HandleSimulateScanClicked()
    {
        ResolveReferences();
        if (dataLoader == null || !dataLoader.HasQrRegistry || dataLoader.QrRegistry == null)
            return;

        var index = locationDropdown != null ? locationDropdown.value : 0;
        var entries = dataLoader.QrRegistry.locations;
        if (entries == null || index < 0 || index >= entries.Length)
            return;

        var entry = entries[index];
        if (entry == null || string.IsNullOrEmpty(entry.payload))
            return;

        if (triggerBridge != null)
            triggerBridge.TryTriggerFromPayload(entry.payload);
        else if (scanService != null)
            scanService.SubmitPayloadForTesting(entry.payload);

        RefreshStatus();
    }

    void RefreshStatus()
    {
        if (statusText == null)
            return;

        ResolveReferences();
        if (dataLoader == null || !dataLoader.HasQrRegistry)
        {
            statusText.text = "QR registry yok";
            return;
        }

        var bridgeStatus = triggerBridge != null ? triggerBridge.StatusMessage : "Köprü yok";
        var scanStatus = scanService != null ? scanService.StatusMessage : "Tarayıcı yok";
        statusText.text = bridgeStatus + "\n" + scanStatus;
    }
}

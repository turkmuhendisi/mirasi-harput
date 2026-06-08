using System;

[Serializable]
public class LocationQrEntry
{
    public string locationId;
    public string displayName;
    public string payload;
    public string qrImageFile;
    public string placementNotes;
    public string[] alternatePayloads;
}

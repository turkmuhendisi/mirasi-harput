using System;

[Serializable]
public class LocationData
{
    public string id;
    public string name;
    public string shortDescription;
    public string longDescription;
    public double latitude;
    public double longitude;
    public float triggerRadiusMeters;
    public string npcId;
    public string relatedQuestId;
    public string imageKey;
    public bool isStartPoint;
    public bool isFinalPoint;
}

using System;
using UnityEngine;

[Serializable]
public class LocationModelTransform
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class LocationModel
{
    public string id;
    public string title;
    public string qrPayload;
    public string description;
    public string voiceText;
    public string modelPath;
    public string audioPath;
    public float modelScale = 1f;
    public LocationModelTransform modelRotation = new LocationModelTransform { x = 0f, y = 180f, z = 0f };
    public LocationModelTransform modelPosition = new LocationModelTransform { x = 0f, y = 0f, z = -1.5f };
}

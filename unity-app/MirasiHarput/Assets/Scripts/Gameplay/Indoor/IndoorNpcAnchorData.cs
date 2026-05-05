using System;
using UnityEngine;

[Serializable]
public class IndoorNpcAnchorData
{
    public string npcId;
    public string npcName;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public bool isPlaced;
}

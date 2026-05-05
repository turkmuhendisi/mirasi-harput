using System;

[Serializable]
public class QuestData
{
    public string id;
    public string title;
    public string description;
    public string objectiveText;
    public string locationId;
    public string nextLocationId;
    public int rewardPoint;
    public string badgeId;
    public string npcDialogueStart;
    public string npcDialogueComplete;
    public string questionText;
    public string correctAnswer;
    public string[] wrongAnswers;
}

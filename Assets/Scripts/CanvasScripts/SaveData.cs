using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public bool hasData;

    public int slotIndex;

    public int storyPercent;

    public int coins;

    public int trinketsUnlocked;
    public int trinketsTotal;

    public int cosmeticsUnlocked;
    public int cosmeticsTotal;

    public int bonusPercent;

    public string lastSceneName;

    public List<string> collectedTrinketIds = new List<string>();
}
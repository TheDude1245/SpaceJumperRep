using System;
using System.Collections.Generic;

[Serializable]
public class GlobalProfileData
{
    public bool hasProfile;

    public List<string> unlockedCharacterIds = new List<string>();
    public List<CharacterProgressData> characterProgress = new List<CharacterProgressData>();

    public List<string> selectedPartyCharacterIds = new List<string>();
}

[Serializable]
public class CharacterProgressData
{
    public string characterId;
    public int level;
}
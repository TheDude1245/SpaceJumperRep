using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Game/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public CharacterData[] characters;

    public CharacterData GetCharacterById(string characterId)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && characters[i].characterId == characterId)
            {
                return characters[i];
            }
        }

        return null;
    }

    public CharacterData[] GetCharactersByElement(CharacterElement element)
    {
        int count = 0;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && characters[i].element == element)
                count++;
        }

        CharacterData[] result = new CharacterData[count];
        int index = 0;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && characters[i].element == element)
            {
                result[index] = characters[i];
                index++;
            }
        }

        return result;
    }
}
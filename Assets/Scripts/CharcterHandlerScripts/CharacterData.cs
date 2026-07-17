using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterId;
    public string displayName;
    public CharacterElement element;

    [Header("Base Stats")]
    public int baseShield;
    public int baseHeart;
    public int baseSword;
    public int baseClover;

    [Header("Description")]
    [TextArea]
    public string description;

    [Header("Visuals")]
    public Sprite characterPoseImage;
    public Sprite characterIconImage;
    public Sprite elementStandImage;

    [Header("Gameplay")]
    public GameObject characterPrefab;
}
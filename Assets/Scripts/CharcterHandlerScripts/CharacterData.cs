
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
    [TextArea(2, 5)]
    public string description;

    [Header("Character Artwork")]
    public Sprite characterPoseImage;
    public Sprite silhouettePoseImage;
    public Sprite selectedImage;
    public Sprite characterIconImage;

    [Header("Elemental Artwork")]
    public Sprite elementStandImage;

    [Header("Character Audio")]
    public AudioClip selectionVoiceLine;

    [Header("Gameplay")]
    public GameObject characterPrefab;
}

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character")]
public class CharacterSO : ScriptableObject
{
    public Transform prefab;
    public string playableCharacterName;
    public float walkSpeed;
    public float runSpeed;
    public float maxHealth;
}

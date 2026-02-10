using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/AvailableCharacterList")]
public class AvailableCharacterListSO : ScriptableObject
{
    public List<CharacterSO> list;
}

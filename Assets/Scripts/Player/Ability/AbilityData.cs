using UnityEngine;
using System.Collections.Generic;
using static Define;
[CreateAssetMenu(menuName = "Ability/Ability")]
public class AbilityData : ScriptableObject
{
    public Define.AbilityID abilityID;
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;
    public int maxStack = 5;
    
    public AbilityEffect GetEffect()
    {
        return AbilityFactory.CreateEffect(abilityID);
    }
}